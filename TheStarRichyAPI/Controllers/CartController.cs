using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheStarRichyApi.Models;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// ดึงข้อมูลตะกร้าสินค้า
        /// GET: /api/Cart/get
        /// </summary>
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey",
                        Data = new CartData()
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Member not found",
                        Data = new CartData()
                    });
                }

                var cart = await _cartService.GetCartAsync(memberCode);

                return Ok(new CartResponse
                {
                    Success = true,
                    Message = "ดึงข้อมูลตะกร้าสำเร็จ",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new CartResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการดึงข้อมูลตะกร้า",
                    Data = new CartData()
                });
            }
        }

        /// <summary>
        /// เพิ่มสินค้าลงตะกร้า
        /// POST: /api/Cart/add
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey",
                        Data = new CartData()
                    });
                }

                // Validate request
                if (request == null || string.IsNullOrEmpty(request.ProductID))
                {
                    return BadRequest(new CartResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ถูกต้อง",
                        Data = new CartData()
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Member not found",
                        Data = new CartData()
                    });
                }

                // เพิ่มสินค้า (SP จะตรวจสอบและล้างตะกร้าถ้า DL/Center ไม่ตรง)
                var cartId = await _cartService.AddToCartAsync(memberCode, request);

                // ดึงตะกร้าใหม่
                var cart = await _cartService.GetCartAsync(memberCode);

                return Ok(new CartResponse
                {
                    Success = true,
                    Message = "เพิ่มสินค้าลงตะกร้าเรียบร้อย",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return StatusCode(500, new CartResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการเพิ่มสินค้า",
                    Data = new CartData()
                });
            }
        }

        /// <summary>
        /// อัพเดทจำนวนสินค้า
        /// POST: /api/Cart/update
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request)
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey",
                        Data = new CartData()
                    });
                }

                // Validate request
                if (request == null || string.IsNullOrEmpty(request.ProductID))
                {
                    return BadRequest(new CartResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ถูกต้อง",
                        Data = new CartData()
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Member not found",
                        Data = new CartData()
                    });
                }

                // อัพเดท
                var success = await _cartService.UpdateCartAsync(memberCode, request);

                if (!success)
                {
                    return BadRequest(new CartResponse
                    {
                        Success = false,
                        Message = "ไม่สามารถอัพเดทสินค้าได้",
                        Data = new CartData()
                    });
                }

                // ดึงตะกร้าใหม่
                var cart = await _cartService.GetCartAsync(memberCode);

                return Ok(new CartResponse
                {
                    Success = true,
                    Message = "อัพเดทสินค้าเรียบร้อย",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                return StatusCode(500, new CartResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการอัพเดทสินค้า",
                    Data = new CartData()
                });
            }
        }

        /// <summary>
        /// ⭐ NEW - อัพเดท DL และ Center
        /// POST: /api/Cart/update-dlcenter
        /// </summary>
        [HttpPost("update-dlcenter")]
        public async Task<IActionResult> UpdateDLCenter([FromBody] UpdateCartDLCenterRequest request)
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey",
                        Data = new CartData()
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Member not found",
                        Data = new CartData()
                    });
                }

                // อัพเดท DL/Center
                var success = await _cartService.UpdateCartDLCenterAsync(memberCode, request);

                if (!success)
                {
                    return BadRequest(new CartResponse
                    {
                        Success = false,
                        Message = "ไม่สามารถอัพเดทข้อมูล DL/Center ได้",
                        Data = new CartData()
                    });
                }

                // ดึงตะกร้าใหม่
                var cart = await _cartService.GetCartAsync(memberCode);

                return Ok(new CartResponse
                {
                    Success = true,
                    Message = "อัพเดทข้อมูล DL/Center เรียบร้อย",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DL/Center");
                return StatusCode(500, new CartResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการอัพเดทข้อมูล DL/Center",
                    Data = new CartData()
                });
            }
        }

        /// <summary>
        /// ลบสินค้าออกจากตะกร้า
        /// DELETE: /api/Cart/remove/{productId}
        /// </summary>
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey",
                        Data = new CartData()
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Member not found",
                        Data = new CartData()
                    });
                }

                // ลบสินค้า
                var success = await _cartService.RemoveFromCartAsync(memberCode, productId);

                if (!success)
                {
                    return BadRequest(new CartResponse
                    {
                        Success = false,
                        Message = "ไม่สามารถลบสินค้าได้",
                        Data = new CartData()
                    });
                }

                // ดึงตะกร้าใหม่
                var cart = await _cartService.GetCartAsync(memberCode);

                return Ok(new CartResponse
                {
                    Success = true,
                    Message = "ลบสินค้าออกจากตะกร้าเรียบร้อย",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                return StatusCode(500, new CartResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการลบสินค้า",
                    Data = new CartData()
                });
            }
        }

        /// <summary>
        /// ล้างตะกร้าสินค้าทั้งหมด
        /// POST: /api/Cart/clear
        /// </summary>
        [HttpPost("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey",
                        Data = new CartData()
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CartResponse
                    {
                        Success = false,
                        Message = "Member not found",
                        Data = new CartData()
                    });
                }

                // ล้างตะกร้า
                var success = await _cartService.ClearCartAsync(memberCode);

                if (!success)
                {
                    return BadRequest(new CartResponse
                    {
                        Success = false,
                        Message = "ไม่สามารถล้างตะกร้าได้",
                        Data = new CartData()
                    });
                }

                return Ok(new CartResponse
                {
                    Success = true,
                    Message = "ล้างตะกร้าเรียบร้อย",
                    Data = new CartData()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new CartResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการล้างตะกร้า",
                    Data = new CartData()
                });
            }
        }

        /// <summary>
        /// บันทึกคำสั่งซื้อ (Checkout)
        /// POST: /api/Cart/checkout
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                // ตรวจสอบ Passkey
                if (!await _cartService.ValidatePasskeyAsync())
                {
                    return Unauthorized(new CheckoutResponse
                    {
                        Success = false,
                        Message = "Invalid Passkey"
                    });
                }

                // ดึง MemberCode จาก JWT
                string memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(memberCode))
                {
                    return Unauthorized(new CheckoutResponse
                    {
                        Success = false,
                        Message = "Member not found"
                    });
                }

                // ดึงข้อมูลตะกร้าก่อน checkout
                var cart = await _cartService.GetCartAsync(memberCode);

                if (cart.CartID == 0 || cart.Items.Count == 0)
                {
                    return BadRequest(new CheckoutResponse
                    {
                        Success = false,
                        Message = "ไม่มีสินค้าในตะกร้า"
                    });
                }

                // Checkout
                var orderId = await _cartService.CheckoutAsync(memberCode);

                if (string.IsNullOrEmpty(orderId))
                {
                    return BadRequest(new CheckoutResponse
                    {
                        Success = false,
                        Message = "ไม่สามารถบันทึกคำสั่งซื้อได้"
                    });
                }

                return Ok(new CheckoutResponse
                {
                    Success = true,
                    Message = "บันทึกคำสั่งซื้อเรียบร้อย",
                    OrderID = orderId,
                    TotalAmount = cart.TotalAmount,
                    TotalPV = cart.TotalPV
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return StatusCode(500, new CheckoutResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการบันทึกคำสั่งซื้อ"
                });
            }
        }
    }
}