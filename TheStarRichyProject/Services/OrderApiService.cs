using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TheStarRichyProject.Models;
using static TheStarRichyProject.Controllers.ordersController;

namespace TheStarRichyProject.Services
{
    public interface IOrderApiService
    {
        Task<CheckoutInfoResponse> SaveCheckoutInfoAsync(string token, string passkey, CheckoutInfoRequest request);
        Task<OrderSummaryResponse> GetOrderSummaryAsync(string token, string passkey, string orderID);
        Task<PaymentResponse> CreatePaymentAsync(string token, string passkey, PaymentRequest request);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string token, string passkey, string transactionID);
        Task<MemberAddressesResponse> GetMemberAddressesAsync(string token, string passkey);
        Task<MemberFavoriteAddressesResponse> GetMemberFavoriteAddressesAsync(string token, string passkey);
        Task<BranchesResponse> GetBranchesAsync(string token, string passkey, string provinceCode = null);
        Task<OrderResponse> ConfirmOrderAsync(string token, string passkey, string orderID);
        Task<CenterResponse> FindCenterFromApi(string token, string passkey, string centercode);
    }

    public class OrderApiService : IOrderApiService
    {
        private readonly IConfiguration _config;

        public OrderApiService(IConfiguration config)
        {
            _config = config;
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

        public async Task<CheckoutInfoResponse> SaveCheckoutInfoAsync(string token, string passkey, CheckoutInfoRequest request)
        {
            try
            {
                var client = CreateClient();
                var restRequest = new RestRequest("/Order/checkout-info", Method.Post);
                restRequest.AddHeader("Authorization", $"Bearer {token}");
                restRequest.AddHeader("X-Passkey", passkey);
                restRequest.AddJsonBody(request);

                var response = await client.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CheckoutInfoResponse>(response.Content);
                }

                return new CheckoutInfoResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถบันทึกข้อมูลได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new CheckoutInfoResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<OrderSummaryResponse> GetOrderSummaryAsync(string token, string passkey, string orderID)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest($"/Order/summary/{orderID}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<OrderSummaryResponse>(response.Content);
                }

                return new OrderSummaryResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถดึงข้อมูลได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new OrderSummaryResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponse> CreatePaymentAsync(string token, string passkey, PaymentRequest request)
        {
            try
            {
                var client = CreateClient();
                var restRequest = new RestRequest("/Order/create-payment", Method.Post);
                restRequest.AddHeader("Authorization", $"Bearer {token}");
                restRequest.AddHeader("X-Passkey", passkey);
                restRequest.AddJsonBody(request);

                var response = await client.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<PaymentResponse>(response.Content);
                }

                return new PaymentResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถสร้างการชำระเงินได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string token, string passkey, string transactionID)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest($"/Order/payment-status/{transactionID}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<PaymentStatusResponse>(response.Content);
                }

                return new PaymentStatusResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถดึงข้อมูลได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new PaymentStatusResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<MemberAddressesResponse> GetMemberAddressesAsync(string token, string passkey)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/Member/member-addresses", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Deserialize with error handling
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var apiAddresses = JsonConvert.DeserializeObject<List<ApiMemberAddress>>(response.Content, settings);

                    // แปลงเป็น MemberAddressData และจัดการกับ object type
                    var memberAddresses = apiAddresses?.Select(a => new MemberAddressData
                    {
                        AddressLine = a.Address,
                        Province = a.Province,
                        District = GetStringValue(a.Amphoe),
                        SubDistrict = GetStringValue(a.Tambon),
                        PostalCode = a.ZipCode,
                        FullAddress = BuildFullAddress(a)
                    }).ToList() ?? new List<MemberAddressData>();

                    return new MemberAddressesResponse
                    {
                        Success = true,
                        Message = "Success",
                        Data = memberAddresses
                    };
                }

                return new MemberAddressesResponse
                {
                    Success = false,
                    Message = response.ErrorMessage ?? "Failed to get addresses",
                    Data = new List<MemberAddressData>()
                };
            }
            catch (Exception ex)
            {
                return new MemberAddressesResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new List<MemberAddressData>()
                };
            }
        }

        public async Task<BranchesResponse> GetBranchesAsync(string token, string passkey, string provinceCode = null)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/Member/branchs", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                if (!string.IsNullOrEmpty(provinceCode))
                {
                    request.AddQueryParameter("provinceCode", provinceCode);
                }

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Deserialize เป็น List<BranchData> โดยตรง เพราะ API return array
                    var branches = JsonConvert.DeserializeObject<List<BranchData>>(response.Content);

                    return new BranchesResponse
                    {
                        Success = true,
                        Data = branches ?? new List<BranchData>()
                    };
                }

                return new BranchesResponse
                {
                    Success = false,
                    Data = new List<BranchData>()
                };
            }
            catch (Exception ex)
            {
                // อาจจะ log exception ไว้ด้วย
                return new BranchesResponse
                {
                    Success = false,
                    Data = new List<BranchData>()
                };
            }
        }

        public async Task<OrderResponse> ConfirmOrderAsync(string token, string passkey, string orderID)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest($"/Order/confirm/{orderID}", Method.Post);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<OrderResponse>(response.Content);
                }

                return new OrderResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถยืนยันคำสั่งซื้อได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<MemberFavoriteAddressesResponse> GetMemberFavoriteAddressesAsync(string token, string passkey)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/member/member-favorite-addresses", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<MemberFavoriteAddressesResponse>(response.Content);
                }

                return new MemberFavoriteAddressesResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถดึงข้อมูลได้: {response.StatusCode}",
                    Data = new List<MemberFavoriteAddressData>()
                };
            }
            catch (Exception ex)
            {
                return new MemberFavoriteAddressesResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}",
                    Data = new List<MemberFavoriteAddressData>()
                };
            }
        }
        public async Task<CenterResponse> FindCenterFromApi(string token, string passkey, string centercode)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest($"/Product/findcenter/{centercode}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CenterResponse>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Helper methods
        private string GetStringValue(object value)
        {
            if (value == null) return string.Empty;
            if (value is string str) return str;
            return string.Empty;
        }

        private string BuildFullAddress(ApiMemberAddress a)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(a.Address)) parts.Add(a.Address);

            var tambon = GetStringValue(a.Tambon);
            if (!string.IsNullOrWhiteSpace(tambon)) parts.Add(tambon);

            var amphoe = GetStringValue(a.Amphoe);
            if (!string.IsNullOrWhiteSpace(amphoe)) parts.Add(amphoe);

            if (!string.IsNullOrWhiteSpace(a.Province)) parts.Add(a.Province);
            if (!string.IsNullOrWhiteSpace(a.ZipCode)) parts.Add(a.ZipCode);

            return string.Join(" ", parts);
        }
    }
}