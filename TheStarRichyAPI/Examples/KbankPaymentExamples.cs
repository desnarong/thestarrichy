using TheStarRichyApi.Models.Kbank;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Examples
{
    /// <summary>
    /// ตัวอย่างการใช้งาน Kbank QR Payment Service
    /// Example usage of Kbank QR Payment Service
    /// </summary>
    public class KbankPaymentExamples
    {
        private readonly IKbankQrPaymentService _qrPaymentService;
        private readonly ILogger<KbankPaymentExamples> _logger;

        public KbankPaymentExamples(
            IKbankQrPaymentService qrPaymentService,
            ILogger<KbankPaymentExamples> logger)
        {
            _qrPaymentService = qrPaymentService;
            _logger = logger;
        }

        /// <summary>
        /// Example 1: Create QR Payment for Order
        /// ตัวอย่างที่ 1: สร้าง QR Payment สำหรับ Order
        /// </summary>
        public async Task<(string TransactionId, string QrCode)> CreateOrderPaymentAsync(
            int orderId, 
            decimal amount, 
            string customerName)
        {
            try
            {
                var request = new QrPaymentRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    TxnAmount = amount,
                    Reference1 = $"ORDER-{orderId}",
                    Reference2 = customerName,
                    Reference3 = DateTime.Now.ToString("yyyyMMdd"),
                    Metadata = $"{{\"orderId\": {orderId}, \"type\": \"order\"}}"
                };

                var result = await _qrPaymentService.CreateQrPaymentAsync(request);

                if (result.StatusCode == "00")
                {
                    _logger.LogInformation("QR Payment created successfully for Order {OrderId}", orderId);
                    
                    // TODO: Save to database
                    // await SavePaymentTransactionToDb(result.PartnerTxnUid, orderId, amount);

                    return (result.PartnerTxnUid, result.QrCode ?? string.Empty);
                }
                else
                {
                    throw new Exception($"Failed to create QR payment: {result.ErrorDesc}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order payment for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Example 2: Check Payment Status
        /// ตัวอย่างที่ 2: ตรวจสอบสถานะการชำระเงิน
        /// </summary>
        public async Task<string> CheckPaymentStatusAsync(string transactionId)
        {
            try
            {
                var request = new QrInquiryRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    OrigPartnerTxnUid = transactionId
                };

                var result = await _qrPaymentService.InquiryPaymentAsync(request);

                if (result.StatusCode == "00")
                {
                    _logger.LogInformation(
                        "Payment status for {TransactionId}: {Status}", 
                        transactionId, 
                        result.TxnStatus);

                    return result.TxnStatus ?? "UNKNOWN";
                }
                else
                {
                    throw new Exception($"Failed to check payment status: {result.ErrorDesc}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for {TransactionId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Example 3: Complete Payment Flow with Auto Status Check
        /// ตัวอย่างที่ 3: กระบวนการชำระเงินแบบครบวงจร พร้อมเช็คสถานะอัตโนมัติ
        /// </summary>
        public async Task<bool> ProcessPaymentWithAutoCheckAsync(
            int orderId, 
            decimal amount, 
            string customerName,
            int maxWaitSeconds = 300) // Wait max 5 minutes
        {
            try
            {
                // Step 1: Create QR Payment
                var (transactionId, qrCode) = await CreateOrderPaymentAsync(orderId, amount, customerName);
                
                _logger.LogInformation(
                    "QR Payment created. TransactionId: {TransactionId}, waiting for payment...", 
                    transactionId);

                // Step 2: Wait and check status periodically
                var startTime = DateTime.UtcNow;
                var checkInterval = TimeSpan.FromSeconds(10); // Check every 10 seconds

                while ((DateTime.UtcNow - startTime).TotalSeconds < maxWaitSeconds)
                {
                    var status = await CheckPaymentStatusAsync(transactionId);

                    if (status == "PAID")
                    {
                        _logger.LogInformation(
                            "Payment successful for Order {OrderId}, TransactionId: {TransactionId}", 
                            orderId, 
                            transactionId);

                        // TODO: Update order status in database
                        // await UpdateOrderStatusToP aid(orderId, transactionId);

                        return true;
                    }
                    else if (status == "CANCELLED" || status == "EXPIRED")
                    {
                        _logger.LogWarning(
                            "Payment {Status} for Order {OrderId}", 
                            status, 
                            orderId);
                        return false;
                    }

                    // Wait before next check
                    await Task.Delay(checkInterval);
                }

                // Timeout
                _logger.LogWarning(
                    "Payment timeout for Order {OrderId} after {Seconds} seconds", 
                    orderId, 
                    maxWaitSeconds);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Example 4: Cancel Unpaid Payment
        /// ตัวอย่างที่ 4: ยกเลิกการชำระเงินที่ยังไม่จ่าย
        /// </summary>
        public async Task<bool> CancelUnpaidPaymentAsync(string transactionId)
        {
            try
            {
                // Check current status first
                var currentStatus = await CheckPaymentStatusAsync(transactionId);

                if (currentStatus == "PAID")
                {
                    _logger.LogWarning(
                        "Cannot cancel paid transaction {TransactionId}", 
                        transactionId);
                    return false;
                }

                // Cancel the payment
                var request = new QrCancelRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    OrigPartnerTxnUid = transactionId
                };

                var result = await _qrPaymentService.CancelPaymentAsync(request);

                if (result.StatusCode == "00")
                {
                    _logger.LogInformation("Payment cancelled successfully: {TransactionId}", transactionId);
                    
                    // TODO: Update database
                    // await UpdatePaymentStatusToC ancelled(transactionId);

                    return true;
                }
                else
                {
                    throw new Exception($"Failed to cancel payment: {result.ErrorDesc}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment {TransactionId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Example 5: Get Payment Details with Inquiry
        /// ตัวอย่างที่ 5: ดึงรายละเอียดการชำระเงิน
        /// </summary>
        public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(string transactionId)
        {
            try
            {
                var request = new QrInquiryRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    OrigPartnerTxnUid = transactionId
                };

                var result = await _qrPaymentService.InquiryPaymentAsync(request);

                if (result.StatusCode == "00")
                {
                    return new PaymentDetailsDto
                    {
                        TransactionId = result.PartnerTxnUid,
                        Status = result.TxnStatus ?? "UNKNOWN",
                        Amount = decimal.Parse(result.TxnAmount ?? "0"),
                        Currency = result.TxnCurrencyCode ?? "THB",
                        Reference1 = result.Reference1,
                        Reference2 = result.Reference2,
                        Channel = result.Channel,
                        TransactionNo = result.TxnNo
                    };
                }
                else
                {
                    throw new Exception($"Failed to get payment details: {result.ErrorDesc}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment details for {TransactionId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Example 6: Create Payment with Custom QR Type
        /// ตัวอย่างที่ 6: สร้างการชำระเงินด้วย QR Type แบบกำหนดเอง
        /// </summary>
        public async Task<string> CreateCustomQrPaymentAsync(
            decimal amount,
            string qrType = "3", // "3" = Thai QR
            string reference = "")
        {
            try
            {
                var request = new QrPaymentRequest
                {
                    PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
                    TxnAmount = amount,
                    QrType = qrType,
                    Reference1 = reference,
                    TxnCurrencyCode = "THB"
                };

                var result = await _qrPaymentService.CreateQrPaymentAsync(request);

                if (result.StatusCode == "00" && result.QrCode != null)
                {
                    return result.QrCode;
                }
                else
                {
                    throw new Exception($"Failed to create custom QR payment: {result.ErrorDesc}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating custom QR payment");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO for Payment Details
    /// </summary>
    public class PaymentDetailsDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "THB";
        public string? Reference1 { get; set; }
        public string? Reference2 { get; set; }
        public string? Channel { get; set; }
        public string? TransactionNo { get; set; }
    }
}
