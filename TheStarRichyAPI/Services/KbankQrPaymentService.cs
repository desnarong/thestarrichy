using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TheStarRichyApi.Models.Kbank;

namespace TheStarRichyApi.Services
{
    public interface IKbankQrPaymentService
    {
        Task<QrPaymentResponse> CreateQrPaymentAsync(QrPaymentRequest request);
        Task<QrInquiryResponse> InquiryPaymentAsync(QrInquiryRequest request);
        Task<QrCancelResponse> CancelPaymentAsync(QrCancelRequest request);
        Task<QrCancelResponse> VoidPaymentAsync(QrCancelRequest request);
        Task<QrSettlementResponse> SettlementAsync(QrSettlementRequest request);
        string GeneratePartnerTxnUid();
    }
    public class KbankQrPaymentService : IKbankQrPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly KbankSettings _settings;
        private readonly IKbankAuthService _authService;
        private readonly ILogger<KbankQrPaymentService> _logger;

        public KbankQrPaymentService(
            HttpClient httpClient,
            IOptions<KbankSettings> settings,
            IKbankAuthService authService,
            ILogger<KbankQrPaymentService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _authService = authService;
            _logger = logger;
        }

        public async Task<QrPaymentResponse> CreateQrPaymentAsync(QrPaymentRequest request)
        {
            try
            {
                // Auto-fill required fields from settings
                request.PartnerTxnUid = GeneratePartnerTxnUid();
                request.PartnerId = _settings.PartnerId;
                request.PartnerSecret = _settings.PartnerSecret;
                request.MerchantId = _settings.MerchantId;
                request.TerminalId = _settings.TerminalId;
                request.RequestDt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var accessToken = await _authService.GetAccessTokenAsync();
                var url = $"{_settings.BaseUrl}/v1/qrpayment/request";

                var jsonContent = JsonSerializer.Serialize(request);
                _logger.LogInformation("Creating QR Payment. Request: {Request}", jsonContent);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken.AccessToken}");
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to create QR payment. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    throw new Exception($"Failed to create QR payment: {content}");
                }

                var result = JsonSerializer.Deserialize<QrPaymentResponse>(content);
                _logger.LogInformation("QR Payment created successfully. PartnerTxnUid: {PartnerTxnUid}",
                    result?.PartnerTxnUid);

                return result ?? throw new Exception("Invalid response from Kbank");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating QR payment");
                throw;
            }
        }

        public async Task<QrInquiryResponse> InquiryPaymentAsync(QrInquiryRequest request)
        {
            try
            {
                // Auto-fill required fields from settings
                request.PartnerId = _settings.PartnerId;
                request.PartnerSecret = _settings.PartnerSecret;
                request.MerchantId = _settings.MerchantId;
                request.TerminalId = _settings.TerminalId;
                request.RequestDt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var accessToken = await _authService.GetAccessTokenAsync();
                var url = $"{_settings.BaseUrl}/v1/qrpayment/v4/inquiry";

                var jsonContent = JsonSerializer.Serialize(request);
                _logger.LogInformation("Inquiring QR Payment. Request: {Request}", jsonContent);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to inquiry payment. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    throw new Exception($"Failed to inquiry payment: {content}");
                }

                var result = JsonSerializer.Deserialize<QrInquiryResponse>(content);
                _logger.LogInformation("Payment inquiry completed. Status: {Status}", result?.TxnStatus);

                return result ?? throw new Exception("Invalid response from Kbank");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inquiring payment");
                throw;
            }
        }

        public async Task<QrCancelResponse> CancelPaymentAsync(QrCancelRequest request)
        {
            try
            {
                // Auto-fill required fields from settings
                request.PartnerId = _settings.PartnerId;
                request.PartnerSecret = _settings.PartnerSecret;
                request.MerchantId = _settings.MerchantId;
                request.TerminalId = _settings.TerminalId;
                request.RequestDt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var accessToken = await _authService.GetAccessTokenAsync();
                var url = $"{_settings.BaseUrl}/v1/qrpayment/cancel";

                var jsonContent = JsonSerializer.Serialize(request);
                _logger.LogInformation("Canceling QR Payment. Request: {Request}", jsonContent);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to cancel payment. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    throw new Exception($"Failed to cancel payment: {content}");
                }

                var result = JsonSerializer.Deserialize<QrCancelResponse>(content);
                _logger.LogInformation("Payment canceled successfully. PartnerTxnUid: {PartnerTxnUid}",
                    result?.PartnerTxnUid);

                return result ?? throw new Exception("Invalid response from Kbank");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment");
                throw;
            }
        }

        public async Task<QrCancelResponse> VoidPaymentAsync(QrCancelRequest request)
        {
            try
            {
                // Auto-fill required fields from settings
                request.PartnerId = _settings.PartnerId;
                request.PartnerSecret = _settings.PartnerSecret;
                request.MerchantId = _settings.MerchantId;
                request.TerminalId = _settings.TerminalId;
                request.RequestDt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var accessToken = await _authService.GetAccessTokenAsync();
                var url = $"{_settings.BaseUrl}/v1/qrpayment/void";

                var jsonContent = JsonSerializer.Serialize(request);
                _logger.LogInformation("Voiding QR Payment. Request: {Request}", jsonContent);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to void payment. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    throw new Exception($"Failed to void payment: {content}");
                }

                var result = JsonSerializer.Deserialize<QrCancelResponse>(content);
                _logger.LogInformation("Payment voided successfully. PartnerTxnUid: {PartnerTxnUid}",
                    result?.PartnerTxnUid);

                return result ?? throw new Exception("Invalid response from Kbank");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding payment");
                throw;
            }
        }

        public async Task<QrSettlementResponse> SettlementAsync(QrSettlementRequest request)
        {
            try
            {
                // Auto-fill required fields from settings
                request.PartnerId = _settings.PartnerId;
                request.PartnerSecret = _settings.PartnerSecret;
                request.MerchantId = _settings.MerchantId;
                request.TerminalId = _settings.TerminalId;
                request.RequestDt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

                var accessToken = await _authService.GetAccessTokenAsync();
                var url = $"{_settings.BaseUrl}/v1/qrpayment/settlement";

                var jsonContent = JsonSerializer.Serialize(request);
                _logger.LogInformation("Requesting settlement. Request: {Request}", jsonContent);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get settlement. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    throw new Exception($"Failed to get settlement: {content}");
                }

                var result = JsonSerializer.Deserialize<QrSettlementResponse>(content);
                _logger.LogInformation("Settlement completed successfully. Amount: {Amount}",
                    result?.SettlementAmount);

                return result ?? throw new Exception("Invalid response from Kbank");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settlement");
                throw;
            }
        }

        public string GeneratePartnerTxnUid()
        {
            // Generate unique transaction ID
            // Format: PTR + YYYYMMDD + HHMMSS + Random 3 digits
            return $"PTR{DateTime.Now:yyMMddHHmmss}";
        }
    }
}
