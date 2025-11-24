using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheStarRichyApi.Models.Kbank;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KbankPaymentController : ControllerBase
    {
        private readonly IKbankQrPaymentService _qrPaymentService;
        private readonly ILogger<KbankPaymentController> _logger;
        private readonly IKbankWebhookService _webhookService;

        public KbankPaymentController(
            IKbankQrPaymentService qrPaymentService,
            IKbankWebhookService webhookService,
            ILogger<KbankPaymentController> logger)
        {
            _qrPaymentService = qrPaymentService;
            _webhookService = webhookService;
            _logger = logger;
        }
        /// <summary>
        /// ? KBank Webhook Callback Endpoint (Actual Format)
        /// POST: /KbankPayment/webhook
        /// Alternative: POST: /qr/payment-callback
        /// </summary>
        [HttpPost("webhook")]
        [HttpPost("/qr/payment-callback")]  // Alternative route
        [AllowAnonymous]
        public async Task<ActionResult<KbankWebhookResponse>> ReceiveWebhook([FromBody] KbankWebhookRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Webhook received - PartnerTxnUid: {PartnerTxnUid}, StatusCode: {StatusCode}, Amount: {Amount}, TxnNo: {TxnNo}",
                    request.PartnerTxnUid, request.StatusCode, request.TxnAmount, request.TxnNo);

                // Process webhook
                var result = await _webhookService.ProcessWebhookAsync(request);

                if (result.Success)
                {
                    // Return success to KBank (format ตามเอกสาร)
                    return Ok(new KbankWebhookResponse
                    {
                        StatusCode = "00",
                        ErrorCode = null,
                        ErrorDesc = null
                    });
                }
                else
                {
                    // Return error to KBank (ยังคง return 200 OK แต่มี errorCode)
                    _logger.LogWarning("Webhook processing failed: {Message}", result.Message);

                    return Ok(new KbankWebhookResponse
                    {
                        StatusCode = "99",
                        ErrorCode = result.ErrorCode,
                        ErrorDesc = result.ErrorDesc
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");

                // Always return 200 OK to KBank to prevent retry
                return Ok(new KbankWebhookResponse
                {
                    StatusCode = "99",
                    ErrorCode = "E999",
                    ErrorDesc = "Internal error"
                });
            }
        }

        /// <summary>
        /// Test Webhook Endpoint (for development only)
        /// POST: /KbankPayment/webhook/test
        /// </summary>
        [HttpPost("webhook/test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestWebhook([FromBody] KbankWebhookRequest request)
        {
            try
            {
                _logger.LogInformation("Test webhook received");

                var result = await _webhookService.ProcessWebhookAsync(request);

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    orderID = result.OrderID,
                    paymentStatus = result.PaymentStatus,
                    amount = result.Amount,
                    errorCode = result.ErrorCode,
                    errorDesc = result.ErrorDesc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        /// <summary>
        /// Test Webhook Endpoint (for development only)
        /// POST: /KbankPayment/webhook/test
        /// </summary>
        [HttpPost("webhook/mockup")]
        [AllowAnonymous]
        public async Task<IActionResult> MockupWebhook([FromBody] string orderid)
        {
            try
            {
                _logger.LogInformation("Test webhook received");

                var result = await _webhookService.CallUpdatePaymentStatusAsync(orderid);

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    orderID = result.OrderID,
                    paymentStatus = result.PaymentStatus,
                    amount = result.Amount,
                    errorCode = result.ErrorCode,
                    errorDesc = result.ErrorDesc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        /// <summary>
        /// Create QR Payment
        /// </summary>
        [HttpPost("qr/create")]
        public async Task<ActionResult<QrPaymentResponse>> CreateQrPayment([FromBody] CreateQrPaymentDto dto)
        {
            try
            {
                var request = new QrPaymentRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    TxnAmount = dto.Amount,
                    Reference1 = dto.Reference1,
                    Reference2 = dto.Reference2,
                    Reference3 = dto.Reference3,
                    Reference4 = dto.Reference4,
                    Metadata = dto.Metadata
                };

                var result = await _qrPaymentService.CreateQrPaymentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating QR payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Inquiry payment status
        /// </summary>
        [HttpPost("qr/inquiry")]
        public async Task<ActionResult<QrInquiryResponse>> InquiryPayment([FromBody] InquiryPaymentDto dto)
        {
            try
            {
                var request = new QrInquiryRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    OrigPartnerTxnUid = dto.OrigPartnerTxnUid
                };

                var result = await _qrPaymentService.InquiryPaymentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inquiring payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel payment
        /// </summary>
        [HttpPost("qr/cancel")]
        public async Task<ActionResult<QrCancelResponse>> CancelPayment([FromBody] CancelPaymentDto dto)
        {
            try
            {
                var request = new QrCancelRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    OrigPartnerTxnUid = dto.OrigPartnerTxnUid
                };

                var result = await _qrPaymentService.CancelPaymentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Void payment
        /// </summary>
        [HttpPost("qr/void")]
        public async Task<ActionResult<QrCancelResponse>> VoidPayment([FromBody] CancelPaymentDto dto)
        {
            try
            {
                var request = new QrCancelRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    OrigPartnerTxnUid = dto.OrigPartnerTxnUid
                };

                var result = await _qrPaymentService.VoidPaymentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get settlement information
        /// </summary>
        [HttpPost("qr/settlement")]
        public async Task<ActionResult<QrSettlementResponse>> GetSettlement([FromBody] SettlementDto dto)
        {
            try
            {
                var request = new QrSettlementRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid()
                };

                var result = await _qrPaymentService.SettlementAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settlement");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // DTOs for simplified API usage
    public class CreateQrPaymentDto
    {
        public decimal Amount { get; set; }
        public string? Reference1 { get; set; }
        public string? Reference2 { get; set; }
        public string? Reference3 { get; set; }
        public string? Reference4 { get; set; }
        public string? Metadata { get; set; }
    }

    public class InquiryPaymentDto
    {
        public string OrigPartnerTxnUid { get; set; } = string.Empty;
    }

    public class CancelPaymentDto
    {
        public string OrigPartnerTxnUid { get; set; } = string.Empty;
    }

    public class SettlementDto
    {
        // Add any specific settlement fields if needed
    }
}
