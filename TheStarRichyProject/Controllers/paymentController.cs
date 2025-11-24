using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Services;

namespace TheStarRichyProject.Controllers
{
    /// <summary>
    /// Controller สำหรับจัดการ Payment ผ่าน Kbank QR
    /// </summary>
    public class paymentController : Controller
    {
        private readonly IKbankApiClient _kbankClient;
        private readonly ILogger<paymentController> _logger;

        public paymentController(
            IKbankApiClient kbankClient,
            ILogger<paymentController> logger)
        {
            _kbankClient = kbankClient;
            _logger = logger;
        }

        /// <summary>
        /// หน้าแสดง QR Code สำหรับชำระเงิน
        /// GET: /Payment/QrPayment?orderId=123&amount=1000
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QrPayment(string orderId, decimal amount)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId) || amount <= 0)
                {
                    TempData["Error"] = "ข้อมูลไม่ถูกต้อง";
                    return RedirectToAction("Error");
                }

                // เรียก API สร้าง QR Payment
                var result = await _kbankClient.CreateQrPaymentAsync(
                    amount: amount,
                    reference1: orderId,
                    reference2: User.Identity?.Name
                );

                if (result != null && result.StatusCode == "00")
                {
                    // TODO: บันทึก transaction ลง database
                    // await SaveTransactionAsync(orderId, result.PartnerTxnUid, amount);

                    ViewBag.QrCode = result.QrCode;
                    ViewBag.TransactionId = result.PartnerTxnUid;
                    ViewBag.Amount = amount;
                    ViewBag.OrderId = orderId;
                    ViewBag.AccountName = result.AccountName;

                    return View();
                }
                else
                {
                    TempData["Error"] = result?.ErrorDesc ?? "ไม่สามารถสร้าง QR Payment ได้";
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QrPayment action");
                TempData["Error"] = "เกิดข้อผิดพลาดในการสร้าง QR Payment";
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// API สำหรับเช็คสถานะการชำระเงิน (เรียกจาก JavaScript)
        /// GET: /Payment/CheckStatus?transactionId=PTR123
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckStatus(string transactionId)
        {
            try
            {
                if (string.IsNullOrEmpty(transactionId))
                {
                    return Json(new { success = false, message = "Transaction ID is required" });
                }

                var result = await _kbankClient.InquiryPaymentAsync(transactionId);

                if (result != null && result.StatusCode == "00")
                {
                    return Json(new
                    {
                        success = true,
                        status = result.TxnStatus,
                        amount = result.TxnAmount,
                        reference1 = result.Reference1,
                        txnNo = result.TxnNo
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result?.ErrorDesc ?? "Failed to check status"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// ยกเลิกการชำระเงิน
        /// POST: /Payment/Cancel
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Cancel(string transactionId, string orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(transactionId))
                {
                    TempData["Error"] = "Transaction ID is required";
                    return RedirectToAction("Error");
                }

                var result = await _kbankClient.CancelPaymentAsync(transactionId);

                if (result != null && result.StatusCode == "00")
                {
                    // TODO: อัพเดทสถานะใน database
                    // await UpdateTransactionStatusAsync(transactionId, "CANCELLED");

                    TempData["Success"] = "ยกเลิกการชำระเงินเรียบร้อยแล้ว";
                    return RedirectToAction("OrderDetail", "Orders", new { id = orderId });
                }
                else
                {
                    TempData["Error"] = result?.ErrorDesc ?? "ไม่สามารถยกเลิกการชำระเงินได้";
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment");
                TempData["Error"] = "เกิดข้อผิดพลาดในการยกเลิกการชำระเงิน";
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// หน้าแสดงเมื่อชำระเงินสำเร็จ
        /// GET: /Payment/Success?transactionId=PTR123&orderId=ORD123
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Success(string transactionId, string orderId)
        {
            try
            {
                // ตรวจสอบสถานะอีกครั้ง
                var result = await _kbankClient.InquiryPaymentAsync(transactionId);

                if (result != null && result.TxnStatus == "PAID")
                {
                    // TODO: อัพเดทสถานะ order และ process ต่อ
                    // await ProcessPaidOrderAsync(orderId);

                    ViewBag.TransactionId = transactionId;
                    ViewBag.OrderId = orderId;
                    ViewBag.Amount = result.TxnAmount;
                    ViewBag.TxnNo = result.TxnNo;

                    return View();
                }
                else
                {
                    TempData["Error"] = "ไม่พบข้อมูลการชำระเงิน หรือยังไม่ได้ชำระเงิน";
                    return RedirectToAction("QrPayment", new { orderId = orderId, amount = 0 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Success action");
                TempData["Error"] = "เกิดข้อผิดพลาด";
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// หน้าแสดงเมื่อเกิดข้อผิดพลาด
        /// </summary>
        [HttpGet]
        public IActionResult Error()
        {
            ViewBag.ErrorMessage = TempData["Error"] as string ?? "เกิดข้อผิดพลาด";
            return View();
        }

        #region Private Methods

        // TODO: Implement these methods based on your database structure
        
        // private async Task SaveTransactionAsync(string orderId, string transactionId, decimal amount)
        // {
        //     // Save to database
        // }

        // private async Task UpdateTransactionStatusAsync(string transactionId, string status)
        // {
        //     // Update database
        // }

        // private async Task ProcessPaidOrderAsync(string orderId)
        // {
        //     // Process order after successful payment
        // }

        #endregion
    }
}
