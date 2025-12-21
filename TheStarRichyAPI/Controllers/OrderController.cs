using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheStarRichyApi.Models;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    /// <summary>
    /// Order Controller สำหรับระบบ Personal Order 3 Steps
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// บันทึกข้อมูล Checkout (Step 2)
        /// POST: /api/Order/checkout-info
        /// </summary>
        [HttpPost("checkout-info")]
        public async Task<IActionResult> SaveCheckoutInfo([FromBody] CheckoutInfoRequest request)
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _orderService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Member not found"
                    });
                }

                // บันทึกข้อมูล
                var result = await _orderService.SaveCheckoutInfoAsync(memberCode, request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving checkout info");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล"
                });
            }
        }

        /// <summary>
        /// ดึงสรุปคำสั่งซื้อ
        /// GET: /api/Order/summary/{orderID}
        /// </summary>
        [HttpGet("summary/{orderID}")]
        public async Task<IActionResult> GetOrderSummary(string orderID)
        {
            try
            {
                if (!await _orderService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Member not found"
                    });
                }

                var summary = await _orderService.GetOrderSummaryAsync(memberCode, orderID);

                if (summary != null)
                {
                    return Ok(new ApiResponse<OrderSummary>
                    {
                        Success = true,
                        Message = "ดึงข้อมูลสำเร็จ",
                        Data = summary
                    });
                }

                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ไม่พบคำสั่งซื้อ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order summary");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาด"
                });
            }
        }

        /// <summary>
        /// สร้างการชำระเงิน (Step 3)
        /// POST: /api/Order/create-payment
        /// </summary>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                if (!await _orderService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Member not found"
                    });
                }

                var result = await _orderService.CreatePaymentAsync(memberCode, request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการสร้างการชำระเงิน"
                });
            }
        }

        /// <summary>
        /// ตรวจสอบสถานะการชำระเงิน
        /// GET: /api/Order/payment-status/{paymentID}
        /// </summary>
        [HttpGet("payment-status/{paymentID}")]
        public async Task<IActionResult> GetPaymentStatus(string paymentID)
        {
            try
            {
                if (!await _orderService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                var status = await _orderService.GetPaymentStatusAsync(paymentID);

                return Ok(new ApiResponse<PaymentStatusResponse>
                {
                    Success = true,
                    Message = "ดึงสถานะสำเร็จ",
                    Data = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาด"
                });
            }
        }

        /// <summary>
        /// ยืนยันคำสั่งซื้อ (หลังชำระเงินสำเร็จ)
        /// POST: /api/Order/confirm/{orderID}
        /// </summary>
        [HttpPost("confirm/{orderID}")]
        public async Task<IActionResult> ConfirmOrder(string orderID)
        {
            try
            {
                if (!await _orderService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Member not found"
                    });
                }

                var result = await _orderService.ConfirmOrderAsync(memberCode, orderID);

                if (result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "ยืนยันคำสั่งซื้อสำเร็จ"
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ไม่สามารถยืนยันคำสั่งซื้อได้"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาด"
                });
            }
        }
        /// <summary>
        /// อัพโหลดรูปหลักฐานการชำระเงิน
        /// POST: /api/Order/update/{orderID}
        /// </summary>
        [HttpPost("update/{orderID}")]
        public async Task<IActionResult> UpdateOrder(string orderID, string remark)
        {
            try
            {
                if (!await _orderService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Member not found"
                    });
                }

                var result = await _orderService.BankSlipOrderAsync(memberCode, orderID, remark);

                if (result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "ยืนยันคำสั่งซื้อสำเร็จ"
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ไม่สามารถยืนยันคำสั่งซื้อได้"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาด"
                });
            }
        }
    }

    #region Response Models

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    #endregion
}