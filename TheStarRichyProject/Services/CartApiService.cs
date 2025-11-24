using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using TheStarRichyProject.Models;

namespace TheStarRichyProject.Services
{
    public interface ICartApiService
    {
        Task<CartResponse> GetCartAsync(string token, string passkey);
        Task<CartResponse> AddToCartAsync(string token, string passkey, AddToCartRequest request);
        Task<CartResponse> UpdateCartAsync(string token, string passkey, UpdateCartRequest request);
        Task<CartResponse> RemoveFromCartAsync(string token, string passkey, string productId);
        Task<CartResponse> ClearCartAsync(string token, string passkey);
        Task<CheckoutResponse> CheckoutAsync(string token, string passkey);
    }

    public class CartApiService : ICartApiService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartApiService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        private RestClient CreateClient()
        {
            var options = new RestClientOptions(_config["Api:Url"])
            {
                ConfigureMessageHandler = handler =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                }
            };
            return new RestClient(options);
        }

        public async Task<CartResponse> GetCartAsync(string token, string passkey)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/Cart/get", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CartResponse>(response.Content);
                }

                return new CartResponse
                {
                    Success = false,
                    Message = "ไม่สามารถดึงข้อมูลตะกร้าได้",
                    Data = new CartData()
                };
            }
            catch (Exception ex)
            {
                return new CartResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new CartData()
                };
            }
        }

        public async Task<CartResponse> AddToCartAsync(string token, string passkey, AddToCartRequest request)
        {
            try
            {
                var client = CreateClient();
                var restRequest = new RestRequest("/Cart/add", Method.Post);
                restRequest.AddHeader("Authorization", $"Bearer {token}");
                restRequest.AddHeader("X-Passkey", passkey);
                restRequest.AddJsonBody(request);

                var response = await client.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CartResponse>(response.Content);
                }

                return new CartResponse
                {
                    Success = false,
                    Message = "ไม่สามารถเพิ่มสินค้าได้"
                };
            }
            catch (Exception ex)
            {
                return new CartResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CartResponse> UpdateCartAsync(string token, string passkey, UpdateCartRequest request)
        {
            try
            {
                var client = CreateClient();
                var restRequest = new RestRequest("/Cart/update", Method.Post);
                restRequest.AddHeader("Authorization", $"Bearer {token}");
                restRequest.AddHeader("X-Passkey", passkey);
                restRequest.AddJsonBody(request);

                var response = await client.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CartResponse>(response.Content);
                }

                return new CartResponse
                {
                    Success = false,
                    Message = "ไม่สามารถอัพเดทสินค้าได้"
                };
            }
            catch (Exception ex)
            {
                return new CartResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CartResponse> RemoveFromCartAsync(string token, string passkey, string productId)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest($"/Cart/remove/{productId}", Method.Delete);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CartResponse>(response.Content);
                }

                return new CartResponse
                {
                    Success = false,
                    Message = "ไม่สามารถลบสินค้าได้"
                };
            }
            catch (Exception ex)
            {
                return new CartResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CartResponse> ClearCartAsync(string token, string passkey)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/Cart/clear", Method.Post);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CartResponse>(response.Content);
                }

                return new CartResponse
                {
                    Success = false,
                    Message = "ไม่สามารถล้างตะกร้าได้"
                };
            }
            catch (Exception ex)
            {
                return new CartResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CheckoutResponse> CheckoutAsync(string token, string passkey)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/Cart/checkout", Method.Post);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CheckoutResponse>(response.Content);
                }

                return new CheckoutResponse
                {
                    Success = false,
                    Message = "ไม่สามารถบันทึกคำสั่งซื้อได้"
                };
            }
            catch (Exception ex)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}