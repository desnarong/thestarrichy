using RestSharp;
using System.Net;
using TheStarRichyProject.Helper;
using TheStarRichyProject.Models;

namespace TheStarRichyProject.Services
{
    /// <summary>
    /// Service สำหรับเรียกใช้ Product API
    /// ใช้ RestSharp แบบเดียวกับ memberController
    /// </summary>
    public interface IProductApiClient
    {
        Task<List<ProductGroup>?> GetProductGroupsAsync();
        Task<List<Product>?> GetGroupOfProductsAsync(string? groupId = null);
        Task<List<Product>?> GetProductListForTopupAsync();
        Task<MemberForSale?> FindMemberCodeForSaleAsync(string memberCode);
    }

    public class ProductApiClient : IProductApiClient
    {
        private readonly ILogger<ProductApiClient> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductApiClient(
            IConfiguration configuration,
            ILogger<ProductApiClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// สร้าง RestClient ตามแบบ memberController
        /// </summary>
        private RestClient CreateRestClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var options = new RestClientOptions(_config["Api:Url"])
            {
                ThrowOnAnyError = true,
                ConfigureMessageHandler = handler =>
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        // ข้ามการตรวจสอบใบรับรอง (สำหรับทดสอบเท่านั้น)
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                    return httpClientHandler;
                }
            };

            return new RestClient(options);
        }

        /// <summary>
        /// เพิ่ม Headers ตามมาตรฐาน
        /// </summary>
        private void AddHeaders(RestRequest request)
        {
            var passkey = _config["Api:Passkey"];
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[CookieHelper.UserKey];

            request.AddHeader("X-Passkey", passkey);

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            request.AddHeader("Accept", "application/json");
        }

        /// <summary>
        /// ดึงรายการกลุ่มสินค้าทั้งหมด
        /// GET /Product/productgroup
        /// </summary>
        public async Task<List<ProductGroup>?> GetProductGroupsAsync()
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/productgroup", Method.Get);

                AddHeaders(request);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Parse response
                    var result = System.Text.Json.JsonSerializer.Deserialize<List<ProductGroup>>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result;
                }

                _logger.LogError("Failed to get product groups. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, response.Content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product groups");
                return null;
            }
        }

        /// <summary>
        /// ดึงรายการสินค้าตามกลุ่ม
        /// GET /Product/groupofproducts?groupId={groupId}
        /// </summary>
        public async Task<List<Product>?> GetGroupOfProductsAsync(string? groupId = null)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/groupofproducts", Method.Get);

                AddHeaders(request);

                // Add query parameters
                if (string.IsNullOrEmpty(groupId))
                {
                    request.AddQueryParameter("groupId", groupId);
                }

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result;
                }

                _logger.LogError("Failed to get products. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return null;
            }
        }

        /// <summary>
        /// ดึงรายการสินค้าสำหรับ Topup
        /// GET /Product/productlistfortopup
        /// </summary>
        public async Task<List<Product>?> GetProductListForTopupAsync()
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/productlistfortopup", Method.Get);

                AddHeaders(request);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result;
                }

                _logger.LogError("Failed to get topup products. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topup products");
                return null;
            }
        }

        /// <summary>
        /// ค้นหาข้อมูล Member สำหรับซื้อสินค้า
        /// GET /Product/findmembercodeforsale?memberCode={memberCode}
        /// </summary>
        public async Task<MemberForSale?> FindMemberCodeForSaleAsync(string memberCode)
        {
            try
            {
                if (string.IsNullOrEmpty(memberCode))
                {
                    return null;
                }

                var client = CreateRestClient();
                var request = new RestRequest("/Product/findmembercodeforsale", Method.Get);

                AddHeaders(request);

                // Add query parameter
                request.AddQueryParameter("memberCode", memberCode);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<MemberForSale>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result;
                }

                _logger.LogWarning("Member not found: {MemberCode}. Status: {StatusCode}",
                    memberCode, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding member code: {MemberCode}", memberCode);
                return null;
            }
        }
    }

    /// <summary>
    /// Generic API Response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
