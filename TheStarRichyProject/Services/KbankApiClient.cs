using RestSharp;
using System.Net;
using TheStarRichyProject.Helper;

namespace TheStarRichyProject.Services
{
    /// <summary>
    /// Service สำหรับเรียกใช้ Kbank Payment API จาก TheStarRichyApi
    /// ใช้ RestSharp แบบเดียวกับ memberController
    /// </summary>
    public interface IKbankApiClient
    {
        Task<KbankQrCreateResponse?> CreateQrPaymentAsync(decimal amount, string reference1, string? reference2 = null);
        Task<KbankQrInquiryResponse?> InquiryPaymentAsync(string transactionId);
        Task<KbankQrCancelResponse?> CancelPaymentAsync(string transactionId);
    }

    public class KbankApiClient : IKbankApiClient
    {
        private readonly ILogger<KbankApiClient> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KbankApiClient(
            IConfiguration configuration,
            ILogger<KbankApiClient> logger,
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
        /// สร้าง QR Payment
        /// POST /api/KbankPayment/qr/create
        /// </summary>
        public async Task<KbankQrCreateResponse?> CreateQrPaymentAsync(
            decimal amount,
            string reference1,
            string? reference2 = null)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/KbankPayment/qr/create", Method.Post);

                AddHeaders(request);

                // Add body
                var body = new
                {
                    amount = amount,
                    reference1 = reference1,
                    reference2 = reference2
                };
                request.AddJsonBody(body);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<KbankQrCreateResponse>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return result;
                }

                _logger.LogError("Failed to create QR payment. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, response.Content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating QR payment");
                return null;
            }
        }

        /// <summary>
        /// เช็คสถานะการชำระเงิน
        /// POST /api/KbankPayment/qr/inquiry
        /// </summary>
        public async Task<KbankQrInquiryResponse?> InquiryPaymentAsync(string transactionId)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/KbankPayment/qr/inquiry", Method.Post);

                AddHeaders(request);

                // Add body
                var body = new
                {
                    origPartnerTxnUid = transactionId
                };
                request.AddJsonBody(body);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<KbankQrInquiryResponse>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return result;
                }

                _logger.LogError("Failed to inquiry payment. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inquiring payment status");
                return null;
            }
        }

        /// <summary>
        /// ยกเลิกการชำระเงิน
        /// POST /api/KbankPayment/qr/cancel
        /// </summary>
        public async Task<KbankQrCancelResponse?> CancelPaymentAsync(string transactionId)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/KbankPayment/qr/cancel", Method.Post);

                AddHeaders(request);

                // Add body
                var body = new
                {
                    origPartnerTxnUid = transactionId
                };
                request.AddJsonBody(body);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<KbankQrCancelResponse>(
                        response.Content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return result;
                }

                _logger.LogError("Failed to cancel payment. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment");
                return null;
            }
        }
    }

    #region Response Models

    public class KbankQrCreateResponse
    {
        public string? PartnerTxnUid { get; set; }
        public string? PartnerId { get; set; }
        public string? StatusCode { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDesc { get; set; }
        public string? QrCode { get; set; }
        public string? AccountName { get; set; }
    }

    public class KbankQrInquiryResponse
    {
        public string? PartnerTxnUid { get; set; }
        public string? StatusCode { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDesc { get; set; }
        public string? TxnStatus { get; set; }
        public string? TxnAmount { get; set; }
        public string? TxnNo { get; set; }
        public string? Reference1 { get; set; }
        public string? Reference2 { get; set; }
        public string? Channel { get; set; }
    }

    public class KbankQrCancelResponse
    {
        public string? PartnerTxnUid { get; set; }
        public string? StatusCode { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDesc { get; set; }
    }

    #endregion
}
