using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductGroupService _productGroupService;
        private readonly IGroupofProductsService _groupofProductsService;
        private readonly IFindMembercodeForSaleService _findMembercodeForSaleService;
        private readonly IProductListForTopupService _productListForTopupService;
        private readonly IProductListForHoldService _productListForHoldService;
        private readonly IProductListForHurryService _productListForHurryService;
        private readonly IFindCenterMobileService _findCenterService;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(
            IProductGroupService productGroupService,
            IGroupofProductsService groupofProductsService,
            IFindMembercodeForSaleService findMembercodeForSaleService,
            IProductListForTopupService productListForTopupService,
            IProductListForHoldService productListForHoldService,
            IProductListForHurryService productListForHurryService,
            IFindCenterMobileService findCenterMobileService,
            ILogger<ProductController> logger,
            IConfiguration configuration)
        {
            _productGroupService = productGroupService;
            _groupofProductsService = groupofProductsService;
            _findMembercodeForSaleService = findMembercodeForSaleService;
            _productListForTopupService = productListForTopupService;
            _productListForHoldService = productListForHoldService;
            _productListForHurryService = productListForHurryService;
            _findCenterService = findCenterMobileService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// ดึงรายการกลุ่มสินค้า (Product Groups)
        /// GET: /Product/productgroup
        /// </summary>
        [HttpGet("productgroup")]
        public async Task<IActionResult> GetProductGroups()
        {
            try
            {
                var result = await _productGroupService.GetDisplayAsync();

                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลกลุ่มสินค้า",
                        data = new List<object>()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลกลุ่มสินค้าสำเร็จ",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product groups");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลกลุ่มสินค้า",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงรายการสินค้าตามกลุ่ม
        /// GET: /Product/groupofproducts?groupId={groupId}
        /// </summary>
        [HttpGet("groupofproducts")]
        public async Task<IActionResult> GetGroupOfProducts([FromQuery] int? groupId)
        {
            try
            {
                var result = await _groupofProductsService.GetMember2DisplayAsync();

                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลสินค้า",
                        data = new List<object>()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลสินค้าสำเร็จ",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลสินค้า",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงรายการสินค้าสำหรับ Topup (ใช้ ProductListForTopupService)
        /// GET: /Product/productlistfortopup
        /// Query Parameters:
        /// - groupcode: รหัสกลุ่มสินค้า
        /// - producttype: 0=ทั้งหมด, 1=โปรโมชั่น, 2=ขายดี, 3=มาใหม่
        /// - sortorder: 0=ล่าสุด, 1=เก่าสุด, 2=ราคาต่ำ-สูง, 3=ราคาสูง-ต่ำ, 4=PV ต่ำ-สูง, 5=PV สูง-ต่ำ
        /// - productid: รหัสสินค้า (สำหรับค้นหา)
        /// </summary>
        [HttpGet("productlistfortopup")]
        [Authorize] // ต้อง login
        public async Task<IActionResult> GetProductListForTopup(
            [FromQuery] string? groupcode,
            [FromQuery] string? producttype,
            [FromQuery] string? sortorder,
            [FromQuery] string? productid)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                // เรียกใช้ ProductListForTopupService
                var result = await _productListForTopupService.GetDisplayAsync(baseUrl, groupcode, producttype, sortorder, productid);

                // ตรวจสอบผลลัพธ์
                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลสินค้า",
                        data = new List<object>()
                    });
                }

                // ตรวจสอบว่ามี Membercode หรือไม่ (แสดงว่าไม่มี permission)
                //var firstItem = result.FirstOrDefault();
                //if (firstItem != null)
                //{
                //    var dict = firstItem as IDictionary<string, object>;
                //    if (dict != null && dict.ContainsKey("Membercode"))
                //    {
                //        var membercode = dict["Membercode"]?.ToString();
                //        if (string.IsNullOrEmpty(membercode))
                //        {
                //            return Unauthorized(new
                //            {
                //                success = false,
                //                message = "ไม่มีสิทธิ์เข้าถึงข้อมูล",
                //                data = new List<object>()
                //            });
                //        }
                //    }

                //    // ตรวจสอบว่ามี Error หรือไม่
                //    if (dict != null && dict.ContainsKey("Error"))
                //    {
                //        return StatusCode(500, new
                //        {
                //            success = false,
                //            message = dict["Error"]?.ToString(),
                //            data = new List<object>()
                //        });
                //    }
                //}

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลสินค้า Topup สำเร็จ",
                    data = result,
                    filters = new
                    {
                        groupcode = groupcode,
                        producttype = producttype,
                        sortorder = sortorder,
                        productid = productid
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product list for topup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลสินค้า Topup",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงรายการสินค้าสำหรับ Hold (ใช้ ProductListForHoldService)
        /// GET: /api/Product/productlistforhold
        /// Query Parameters:
        /// - groupcode: รหัสกลุ่มสินค้า
        /// - producttype: 0=ทั้งหมด, 1=โปรโมชั่น, 2=ขายดี, 3=มาใหม่
        /// - sortorder: 0=ล่าสุด, 1=เก่าสุด, 2=ราคาต่ำ-สูง, 3=ราคาสูง-ต่ำ, 4=PV ต่ำ-สูง, 5=PV สูง-ต่ำ
        /// - productid: รหัสสินค้า (สำหรับค้นหา)
        /// </summary>
        [HttpGet("productlistforhold")]
        [Authorize] // ต้อง login
        public async Task<IActionResult> GetProductListForHold(
            [FromQuery] string? groupcode,
            [FromQuery] string? producttype,
            [FromQuery] string? sortorder,
            [FromQuery] string? productid)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                // เรียกใช้ ProductListForHoldService
                var result = await _productListForHoldService.GetDisplayAsync(baseUrl, groupcode, producttype, sortorder, productid);

                // ตรวจสอบผลลัพธ์
                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลสินค้า Hold",
                        data = new List<object>()
                    });
                }

                // ตรวจสอบว่ามี Membercode หรือไม่ (แสดงว่าไม่มี permission)
                var firstItem = result.FirstOrDefault();
                if (firstItem != null)
                {
                    var dict = firstItem as IDictionary<string, object>;
                    if (dict != null && dict.ContainsKey("Membercode"))
                    {
                        var membercode = dict["Membercode"]?.ToString();
                        if (string.IsNullOrEmpty(membercode))
                        {
                            return Unauthorized(new
                            {
                                success = false,
                                message = "ไม่มีสิทธิ์เข้าถึงข้อมูล",
                                data = new List<object>()
                            });
                        }
                    }

                    // ตรวจสอบว่ามี Error หรือไม่
                    if (dict != null && dict.ContainsKey("Error"))
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = dict["Error"]?.ToString(),
                            data = new List<object>()
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลสินค้า Hold สำเร็จ",
                    data = result,
                    filters = new
                    {
                        groupcode = groupcode,
                        producttype = producttype,
                        sortorder = sortorder,
                        productid = productid
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product list for hold");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลสินค้า Hold",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ดึงรายการสินค้าสำหรับ Hurry (ใช้ ProductListForHurryService)
        /// GET: /api/Product/productlistforhurry
        /// Query Parameters:
        /// - groupcode: รหัสกลุ่มสินค้า
        /// - producttype: 0=ทั้งหมด, 1=โปรโมชั่น, 2=ขายดี, 3=มาใหม่
        /// - sortorder: 0=ล่าสุด, 1=เก่าสุด, 2=ราคาต่ำ-สูง, 3=ราคาสูง-ต่ำ, 4=PV ต่ำ-สูง, 5=PV สูง-ต่ำ
        /// - productid: รหัสสินค้า (สำหรับค้นหา)
        /// </summary>
        [HttpGet("productlistforhurry")]
        [Authorize] // ต้อง login
        public async Task<IActionResult> GetProductListForHurry(
            [FromQuery] string? groupcode,
            [FromQuery] string? producttype,
            [FromQuery] string? sortorder,
            [FromQuery] string? productid)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                // เรียกใช้ ProductListForHurryService
                var result = await _productListForHurryService.GetDisplayAsync(baseUrl, groupcode, producttype, sortorder, productid);

                // ตรวจสอบผลลัพธ์
                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลสินค้า Hurry",
                        data = new List<object>()
                    });
                }

                // ตรวจสอบว่ามี Membercode หรือไม่ (แสดงว่าไม่มี permission)
                var firstItem = result.FirstOrDefault();
                if (firstItem != null)
                {
                    var dict = firstItem as IDictionary<string, object>;
                    if (dict != null && dict.ContainsKey("Membercode"))
                    {
                        var membercode = dict["Membercode"]?.ToString();
                        if (string.IsNullOrEmpty(membercode))
                        {
                            return Unauthorized(new
                            {
                                success = false,
                                message = "ไม่มีสิทธิ์เข้าถึงข้อมูล",
                                data = new List<object>()
                            });
                        }
                    }

                    // ตรวจสอบว่ามี Error หรือไม่
                    if (dict != null && dict.ContainsKey("Error"))
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = dict["Error"]?.ToString(),
                            data = new List<object>()
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลสินค้า Hurry สำเร็จ",
                    data = result,
                    filters = new
                    {
                        groupcode = groupcode,
                        producttype = producttype,
                        sortorder = sortorder,
                        productid = productid
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product list for hurry");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลสินค้า Hurry",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ค้นหาข้อมูล Member สำหรับซื้อสินค้า
        /// GET: /Product/findmembercodeforsale?memberCode={memberCode}
        /// </summary>
        [HttpGet("findmembercodeforsale")]
        public async Task<IActionResult> FindMemberCodeForSale([FromQuery] string memberCode)
        {
            try
            {
                if (string.IsNullOrEmpty(memberCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุรหัสสมาชิก"
                    });
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var result = await _findMembercodeForSaleService.GetDisplayAsync(memberCode);

                if (result == null || !result.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลสมาชิก",
                        data = new object()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลสมาชิกสำเร็จ",
                    data = result.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding member code");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการค้นหาข้อมูลสมาชิก",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ค้นหาข้อมูล Member สำหรับซื้อสินค้า
        /// GET: /Product/findmembercodeforsale?memberCode={memberCode}
        /// </summary>
        [HttpGet("findcenter")]
        public async Task<IActionResult> FindCenter([FromQuery] string centerCode)
        {
            try
            {
                if (string.IsNullOrEmpty(centerCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุรหัส Center"
                    });
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var result = await _findCenterService.GetDisplayAsync(centerCode);

                if (result == null || !result.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูล Center",
                        data = new object()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "ดึงข้อมูลสมาชิกสำเร็จ",
                    data = result.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding center code");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการค้นหาข้อมูล Center",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Health Check
        /// GET: /Product/health
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.Now,
                service = "Product API"
            });
        }
    }
}