using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheStarRichyProject.Services
{
    /// <summary>
    /// Response model สำหรับ API เช็คสต็อกตามสาขา
    /// </summary>
    public class BranchStockResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<BranchStockItem> Data { get; set; }
    }

    public class BranchStockItem
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Branchcode { get; set; }
    }

    /// <summary>
    /// Response model สำหรับ API ตรวจสอบสินค้าครบหรือไม่
    /// </summary>
    public class CheckStockResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CheckStockData Data { get; set; }
    }

    public class CheckStockData
    {
        public bool IsAllFound { get; set; }
        public string BranchCode { get; set; }
        public List<string> ProductCodes { get; set; }
    }

    /// <summary>
    /// Request model สำหรับตรวจสอบสินค้าในสาขา
    /// </summary>
    public class CheckStockRequestModel
    {
        public string BranchCode { get; set; }
        public List<string> ProductCodes { get; set; }
    }

    /// <summary>
    /// Interface สำหรับบริการเรียก API BranchStock
    /// </summary>
    public interface IBranchStockApiService
    {
        /// <summary>
        /// ดึงรายการสินค้าจาก Branchcode
        /// </summary>
        Task<BranchStockResponse> GetStockByBranchAsync(string token, string passkey, string branchCode);

        /// <summary>
        /// ตรวจสอบว่าสินค้าทั้งหมดใน array มีใน Branchcode นี้หรือไม่
        /// </summary>
        Task<CheckStockResponse> CheckStockByBranchAsync(string token, string passkey, string branchCode, List<string> productCodes);
    }

    /// <summary>
    /// บริการเรียก API BranchStock เพื่อตรวจสอบสต็อกสินค้าตามสาขา
    /// </summary>
    public class BranchStockApiService : IBranchStockApiService
    {
        private readonly IConfiguration _config;

        public BranchStockApiService(IConfiguration config)
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

        /// <summary>
        /// ดึงรายการสินค้าจาก Branchcode
        /// GET: /api/BranchStock/stock-by-branch/{branchCode}
        /// </summary>
        public async Task<BranchStockResponse> GetStockByBranchAsync(string token, string passkey, string branchCode)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest($"/BranchStock/stock-by-branch/{branchCode}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<BranchStockResponse>(response.Content);
                }

                return new BranchStockResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถดึงข้อมูลสต็อกได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new BranchStockResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// ตรวจสอบว่าสินค้าทั้งหมดใน array มีใน Branchcode นี้หรือไม่
        /// POST: /BranchStock/check-stock
        /// Body: { "branchCode": "xxx", "productCodes": ["code1", "code2", ...] }
        /// </summary>
        public async Task<CheckStockResponse> CheckStockByBranchAsync(string token, string passkey, string branchCode, List<string> productCodes)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/BranchStock/check-stock", Method.Post);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var body = new CheckStockRequestModel
                {
                    BranchCode = branchCode,
                    ProductCodes = productCodes
                };
                request.AddStringBody(JsonConvert.SerializeObject(body), ContentType.Json);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<CheckStockResponse>(response.Content);
                }

                return new CheckStockResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถตรวจสอบสต็อกได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new CheckStockResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }
    }
}
