using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheStarRichyProject.Helper;
using TheStarRichyProject.Models;
using TheStarRichyProject.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TheStarRichyProject.Controllers
{
    /// <summary>
    /// Controller สำหรับจัดการ Orders - 4 Steps Checkout
    /// Step 1: Shopping Cart (BuyPersonalOrder)
    /// Step 2: Checkout Info (CheckoutInfo)
    /// Step 3: Payment (Payment)
    /// Step 4: Success (Success)
    /// </summary>
    public class ordersController : Controller
    {
        private readonly ILogger<ordersController> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICartApiService _cartService;
        private readonly IOrderApiService _orderService;
        private readonly IApiService _apiService;

        public ordersController(
            ILogger<ordersController> logger,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ICartApiService cartService,
            IOrderApiService orderService,
            IApiService apiService)
        {
            _logger = logger;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _cartService = cartService;
            _orderService = orderService;
            _apiService = apiService;
        }

        #region Helper Methods

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
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                    return httpClientHandler;
                }
            };

            return new RestClient(options);
        }

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

        private string GetToken()
        {
            return Request.Cookies[CookieHelper.UserKey];
        }

        private string GetPasskey()
        {
            return _config["Api:Passkey"];
        }

        #endregion

        #region Step 1: Shopping Cart (เลือกสินค้า)

        /// <summary>
        /// หน้าสำหรับซื้อสินค้า Personal Order (Step 1)
        /// GET: /Orders/BuyPersonalOrder
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BuyPersonalOrder(
            string? groupcode = null,
            string? producttype = null,
            string? sortorder = null,
            string? search = null)
        {
            try
            {
                var model = new BuyPersonalOrderViewModel
                {
                    SelectedGroupCode = !string.IsNullOrEmpty(groupcode) ? groupcode : null,
                    SearchKeyword = search
                };

                // 1. ดึงกลุ่มสินค้า
                var groups = await GetProductGroupsFromApi();
                if (groups != null)
                {
                    model.ProductGroups = groups;
                }

                // 2. ดึงสินค้า
                var products = await GetProductListForTopupFromApi(groupcode, producttype, sortorder);
                if (products != null)
                {
                    HttpContext.Session.SetString("ProductList", JsonSerializer.Serialize(products));
                    model.Products = products;

                    if (!string.IsNullOrEmpty(search))
                    {
                        model.Products = model.Products
                            .Where(p => p.ProductName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                                       p.ProductId?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
                            .ToList();
                    }
                }

                // 3. ดึง Cart
                var token = GetToken();
                var passkey = GetPasskey();

                if (!string.IsNullOrEmpty(token))
                {
                    var cartResult = await _cartService.GetCartAsync(token, passkey);
                    if (cartResult.Success && cartResult.Data != null)
                    {
                        model.CartItems = cartResult.Data.Items;
                    }
                }

                // 4. ดึงข้อมูล Member
                var memberCode = CookieHelper.GetCookie(_httpContextAccessor, CookieHelper.MemberCodeKey);
                if (!string.IsNullOrEmpty(memberCode))
                {
                    model.CurrentMember = await FindMemberForSaleFromApi(memberCode);
                }
                model.Membercode = memberCode;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BuyPersonalOrder page");
                TempData["Error"] = "เกิดข้อผิดพลาดในการโหลดหน้าสินค้า";
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// หน้าสำหรับซื้อสินค้า Personal Order (Step 1)
        /// GET: /Orders/BuyPersonalOrder
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BuyHholdOrder(
            string? groupcode = null,
            string? producttype = null,
            string? sortorder = null,
            string? search = null)
        {
            try
            {
                var model = new BuyPersonalOrderViewModel
                {
                    SelectedGroupCode = !string.IsNullOrEmpty(groupcode) ? groupcode : null,
                    SearchKeyword = search
                };

                // 1. ดึงกลุ่มสินค้า
                var groups = await GetProductGroupsFromApi();
                if (groups != null)
                {
                    model.ProductGroups = groups;
                }

                // 2. ดึงสินค้า
                var products = await GetProductListForHoldFromApi(groupcode, producttype, sortorder);
                if (products != null)
                {
                    HttpContext.Session.SetString("ProductList", JsonSerializer.Serialize(products));
                    model.Products = products;

                    if (!string.IsNullOrEmpty(search))
                    {
                        model.Products = model.Products
                            .Where(p => p.ProductName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                                       p.ProductId?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
                            .ToList();
                    }
                }

                // 3. ดึง Cart
                var token = GetToken();
                var passkey = GetPasskey();

                if (!string.IsNullOrEmpty(token))
                {
                    var cartResult = await _cartService.GetCartAsync(token, passkey);
                    if (cartResult.Success && cartResult.Data != null)
                    {
                        model.CartItems = cartResult.Data.Items;
                    }
                }

                // 4. ดึงข้อมูล Member
                var memberCode = CookieHelper.GetCookie(_httpContextAccessor, CookieHelper.MemberCodeKey);
                if (!string.IsNullOrEmpty(memberCode))
                {
                    model.CurrentMember = await FindMemberForSaleFromApi(memberCode);
                }
                model.Membercode = memberCode;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BuyPersonalOrder page");
                TempData["Error"] = "เกิดข้อผิดพลาดในการโหลดหน้าสินค้า";
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        public async Task<IActionResult> BuyHurryActive(
            string? groupcode = null,
            string? producttype = null,
            string? sortorder = null,
            string? search = null)
        {
            try
            {
                var model = new BuyPersonalOrderViewModel
                {
                    SelectedGroupCode = !string.IsNullOrEmpty(groupcode) ? groupcode : null,
                    SearchKeyword = search
                };

                // 1. ดึงกลุ่มสินค้า
                var groups = await GetProductGroupsFromApi();
                if (groups != null)
                {
                    model.ProductGroups = groups;
                }

                // 2. ดึงสินค้า
                var products = await GetProductListForHurryFromApi(groupcode, producttype, sortorder);
                if (products != null)
                {
                    HttpContext.Session.SetString("ProductList", JsonSerializer.Serialize(products));
                    model.Products = products;

                    if (!string.IsNullOrEmpty(search))
                    {
                        model.Products = model.Products
                            .Where(p => p.ProductName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                                       p.ProductId?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
                            .ToList();
                    }
                }

                // 3. ดึง Cart
                var token = GetToken();
                var passkey = GetPasskey();

                if (!string.IsNullOrEmpty(token))
                {
                    var cartResult = await _cartService.GetCartAsync(token, passkey);
                    if (cartResult.Success && cartResult.Data != null)
                    {
                        model.CartItems = cartResult.Data.Items;
                    }
                }

                // 4. ดึงข้อมูล Member
                var memberCode = CookieHelper.GetCookie(_httpContextAccessor, CookieHelper.MemberCodeKey);
                if (!string.IsNullOrEmpty(memberCode))
                {
                    model.CurrentMember = await FindMemberForSaleFromApi(memberCode);
                }
                model.Membercode = memberCode;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BuyPersonalOrder page");
                TempData["Error"] = "เกิดข้อผิดพลาดในการโหลดหน้าสินค้า";
                return RedirectToAction("Error", "Home");
            }
        }

        #endregion

        #region Step 2: Checkout Info (กรอกข้อมูลการจัดส่ง)

        /// <summary>
        /// หน้ากรอกข้อมูลการจัดส่งและชำระเงิน (Step 2)
        /// GET: /Orders/CheckoutInfo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckoutInfo()
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "กรุณาเข้าสู่ระบบ";
                    return RedirectToAction("Login", "Auth");
                }

                // ดึงตะกร้า
                var cart = await _cartService.GetCartAsync(token, passkey);

                if (cart == null || !cart.Success || cart.Data == null || cart.Data.Items.Count == 0)
                {
                    TempData["Error"] = "กรุณาเลือกสินค้าก่อนดำเนินการต่อ";
                    return RedirectToAction("BuyPersonalOrder");
                }

                ViewBag.Cart = cart.Data;

                // ดึงที่อยู่ที่เคยใช้
                var addresses = await _orderService.GetMemberAddressesAsync(token, passkey);
                ViewBag.Addresses = addresses.Data ?? new List<MemberAddressData>();

                // ดึงรายการสาขา
                var branches = await _orderService.GetBranchesAsync(token, passkey);
                ViewBag.Branches = branches.Data ?? new List<BranchData>();

                // payment bank
                var paymentbank = await _apiService.GetAsync<List<dynamic>>("/Static/paymentbank");
                ViewBag.PaymentBank  = paymentbank[0];

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CheckoutInfo page");
                TempData["Error"] = "เกิดข้อผิดพลาด: " + ex.Message;
                return RedirectToAction("BuyPersonalOrder");
            }
        }

        /// <summary>
        /// บันทึกข้อมูลการจัดส่ง (API)
        /// POST: /Orders/SaveCheckoutInfo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveCheckoutInfo([FromBody] CheckoutInfoRequest request)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "กรุณา Login ก่อน" });
                }

                // 1. ✅ สร้าง Order จาก Cart (Cart จะ Completed ตรงนี้)
                var checkoutResult = await _cartService.CheckoutAsync(token, passkey);

                if (!checkoutResult.Success)
                {
                    return Json(new { success = false, message = "ไม่สามารถสร้างคำสั่งซื้อได้" });
                }

                var orderID = checkoutResult.OrderID;

                // 2. ✅ บันทึกข้อมูล Checkout (วิธีชำระเงิน, การจัดส่ง)
                request.OrderID = orderID;
                var result = await _orderService.SaveCheckoutInfoAsync(token, passkey, request);

                if (result.Success)
                {
                    // เก็บ OrderID ไว้ใน Session
                    HttpContext.Session.SetString("CurrentOrderID", orderID);

                    return Json(new
                    {
                        success = true,
                        message = "บันทึกข้อมูลสำเร็จ",
                        orderID = orderID  // ⭐ ส่ง orderID ตรงนี้ (ไม่ได้อยู่ใน data)
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveCheckoutInfo");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
        }

        #endregion

        #region Step 3: Payment (ชำระเงิน)

        /// <summary>
        /// หน้าชำระเงิน (Step 3)
        /// GET: /Orders/Payment
        /// </summary>
        public async Task<IActionResult> Payment(string orderID = null)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                // ดึง OrderID จาก parameter หรือ Session
                if (string.IsNullOrEmpty(orderID))
                {
                    orderID = HttpContext.Session.GetString("CurrentOrderID");
                }

                if (string.IsNullOrEmpty(orderID))
                {
                    TempData["Error"] = "ไม่พบข้อมูลคำสั่งซื้อ";
                    return RedirectToAction("BuyPersonalOrder");
                }

                // ✅ ดึงข้อมูลจาก Order (ไม่ใช่ Cart)
                var summary = await _orderService.GetOrderSummaryAsync(token, passkey, orderID);

                if (!summary.Success || summary.Data == null)
                {
                    TempData["Error"] = "ไม่พบข้อมูลคำสั่งซื้อ";
                    return RedirectToAction("BuyPersonalOrder");
                }

                ViewBag.OrderSummary = summary.Data;
                ViewBag.OrderID = orderID;

                // payment bank
                var paymentbank = await _apiService.GetAsync<List<dynamic>>("/Static/paymentbank");
                ViewBag.PaymentBank = paymentbank[0];

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Payment page");
                TempData["Error"] = "เกิดข้อผิดพลาด: " + ex.Message;
                return RedirectToAction("BuyPersonalOrder");
            }
        }

        /// <summary>
        /// สร้างการชำระเงิน (Step 3)
        /// POST: /Orders/CreatePayment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "กรุณา Login ก่อน" });
                }

                var result = await _orderService.CreatePaymentAsync(token, passkey, request);

                if (result.Success && result.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        message = "สร้างการชำระเงินสำเร็จ",
                        paymentID = result.Data.PaymentID,
                        qrCodeData = result.Data.QRCode,
                        amount = result.Data.Amount
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
        }

        /// <summary>
        /// ⭐ ตรวจสอบสถานะการชำระเงิน (Polling ทุก 3 วินาที)
        /// GET: /Orders/GetPaymentStatus
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "กรุณา Login ก่อน" });
                }

                if (string.IsNullOrEmpty(paymentId))
                {
                    return Json(new { success = false, message = "ไม่พบ PaymentID" });
                }

                var result = await _orderService.GetPaymentStatusAsync(token, passkey, paymentId);

                if (result.Success && result.Data != null)
                {
                    // ⭐ ใช้ properties ที่ถูกต้องจาก PaymentStatusData
                    return Json(new
                    {
                        success = true,
                        status = result.Data.Status ?? "Pending",              // มี property นี้
                        paymentStatus = result.Data.PaymentStatus ?? "Pending", // มี property นี้
                        orderStatus = result.Data.OrderStatus ?? "Pending",    // มี property นี้
                        message = result.Data.StatusMessage ?? ""             // มี property นี้
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message ?? "ไม่สามารถตรวจสอบสถานะได้"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return Json(new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาด: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBankSlip([FromBody] BankSlipRequest request)
        {
            var token = GetToken();
            var passkey = GetPasskey();

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "กรุณา Login ก่อน" });
            }

            if (request == null || string.IsNullOrEmpty(request.OrderID) || request.Slips == null || request.Slips.Count == 0)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ครบถ้วน" });
            }

            try
            {
                // 1. กำหนดโฟลเดอร์ที่จะเก็บรูป (เช่น /wwwroot/uploads/slips/ORDER_ID/)
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "slips", request.OrderID);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                List<string> savedFilePaths = new List<string>();

                foreach (var base64String in request.Slips)
                {
                    // แยกส่วนหัว "data:image/jpeg;base64,..." ออกถ้ามี
                    var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    // ตั้งชื่อไฟล์ (ใช้ GUID เพื่อไม่ให้ซ้ำ)
                    string fileName = $"slip_{Guid.NewGuid()}.jpg";
                    string filePath = Path.Combine(folderPath, fileName);

                    // บันทึกไฟล์ลง Disk
                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                    // เก็บ Path สั้นๆ ไว้บันทึกลง Database (ถ้าต้องการ)
                    savedFilePaths.Add($"/uploads/slips/{request.OrderID}/{fileName}");
                }

                // 2. อัปเดตสถานะใน Database (ตัวอย่าง)
                // var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderID == request.OrderID);
                // order.Status = "WaitVerify"; 
                // order.SlipPath = string.Join(";", savedFilePaths); // เก็บหลายรูปคั่นด้วยเครื่องหมาย ;
                // await _context.SaveChangesAsync();

                // ✅ ยืนยันคำสั่งซื้อ (เรียกแค่ตรงนี้!)
                await _orderService.UpdateOrderAsync(token, passkey, request.OrderID, Newtonsoft.Json.JsonConvert.SerializeObject(savedFilePaths));

                return Json(new { success = true, message = "อัปโหลดสลิปเรียบร้อยแล้ว" });
            }
            catch (Exception ex)
            {
                // บันทึก Log ข้อผิดพลาด
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกรูปภาพ: " + ex.Message });
            }
        }

        #endregion

        #region Step 4: Success (สำเร็จ)

        /// <summary>
        /// หน้าสำเร็จ - ยืนยันคำสั่งซื้อ (Step 4)
        /// GET: /Orders/Success
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Success(string orderID = null)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                // ดึง OrderID
                if (string.IsNullOrEmpty(orderID))
                {
                    orderID = HttpContext.Session.GetString("CurrentOrderID");
                }

                if (string.IsNullOrEmpty(orderID))
                {
                    TempData["Error"] = "ไม่พบข้อมูลคำสั่งซื้อ";
                    return RedirectToAction("BuyPersonalOrder");
                }

                // ✅ ยืนยันคำสั่งซื้อ (เรียกแค่ตรงนี้!)
                await _orderService.ConfirmOrderAsync(token, passkey, orderID);

                // ดึงข้อมูลสรุป
                var summary = await _orderService.GetOrderSummaryAsync(token, passkey, orderID);
                ViewBag.OrderSummary = summary.Data;
                ViewBag.OrderID = orderID;

                // Clear session
                HttpContext.Session.Remove("CurrentOrderID");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Success page");
                TempData["Error"] = "เกิดข้อผิดพลาด: " + ex.Message;
                return RedirectToAction("BuyPersonalOrder");
            }
        }

        #endregion

        #region API Helpers

        /// <summary>
        /// ดึงรายการที่อยู่ (API)
        /// GET: /Orders/GetAddresses
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                var result = await _orderService.GetMemberAddressesAsync(token, passkey);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAddresses");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// ดึงรายการที่อยู่ (API)
        /// GET: /Orders/GetFavoriteAddresses
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFavoriteAddresses()
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                var result = await _orderService.GetMemberFavoriteAddressesAsync(token, passkey);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAddresses");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// ดึงรายการสาขา (API)
        /// GET: /Orders/GetBranches
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBranches(string provinceCode = null)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();
                var result = await _orderService.GetBranchesAsync(token, passkey, provinceCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBranches");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Private API Helper Methods
        private async Task<List<dynamic>> GetPaymentBankFromApi()
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Static/paymentbank", Method.Get);
                AddHeaders(request);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<dynamic>>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product groups from API");
                return null;
            }
        }
        private async Task<List<ProductGroup>?> GetProductGroupsFromApi()
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/productgroup", Method.Get);
                AddHeaders(request);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductGroup>>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product groups from API");
                return null;
            }
        }

        private async Task<List<Product>?> GetProductListForTopupFromApi(
            string? groupcode = null,
            string? producttype = null,
            string? sortorder = null)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/productlistfortopup", Method.Get);
                AddHeaders(request);

                if (!string.IsNullOrEmpty(groupcode))
                    request.AddQueryParameter("groupcode", groupcode);

                if (!string.IsNullOrEmpty(producttype))
                    request.AddQueryParameter("producttype", producttype);

                if (!string.IsNullOrEmpty(sortorder))
                    request.AddQueryParameter("sortorder", sortorder);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Product>>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products from API");
                return null;
            }
        }

        private async Task<List<Product>?> GetProductListForHoldFromApi(
            string? groupcode = null,
            string? producttype = null,
            string? sortorder = null)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/productlistforhold", Method.Get);
                AddHeaders(request);

                if (!string.IsNullOrEmpty(groupcode))
                    request.AddQueryParameter("groupcode", groupcode);

                if (!string.IsNullOrEmpty(producttype))
                    request.AddQueryParameter("producttype", producttype);

                if (!string.IsNullOrEmpty(sortorder))
                    request.AddQueryParameter("sortorder", sortorder);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Product>>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products from API");
                return null;
            }
        }

        private async Task<List<Product>?> GetProductListForHurryFromApi(
            string? groupcode = null,
            string? producttype = null,
            string? sortorder = null)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest("/Product/productlistforhurry", Method.Get);
                AddHeaders(request);

                if (!string.IsNullOrEmpty(groupcode))
                    request.AddQueryParameter("groupcode", groupcode);

                if (!string.IsNullOrEmpty(producttype))
                    request.AddQueryParameter("producttype", producttype);

                if (!string.IsNullOrEmpty(sortorder))
                    request.AddQueryParameter("sortorder", sortorder);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Product>>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products from API");
                return null;
            }
        }

        private async Task<MemberForSale?> FindMemberForSaleFromApi(string memberCode)
        {
            try
            {
                var client = CreateRestClient();
                var request = new RestRequest($"/Product/findmembercodeforsale", Method.Get);
                AddHeaders(request);
                request.AddQueryParameter("membercode", memberCode);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<MemberForSale>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding member from API");
                return null;
            }
        }
        /// <summary>
        /// API สำหรับค้นหารหัสสมาชิก (Public endpoint)
        /// GET: /Orders/SearchMemberForSale?membercode=xxx
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchMemberForSale(string dlcode)
        {
            try
            {
                if (string.IsNullOrEmpty(dlcode))
                {
                    return Json(new
                    {
                        success = false,
                        message = "กรุณาระบุรหัสสมาชิก"
                    });
                }

                var memberInfo = await FindMemberForSaleFromApi(dlcode);

                if (memberInfo != null)
                {
                    if (!string.IsNullOrEmpty(memberInfo.DLcode))
                        return Json(new
                        {
                            success = true,
                            data = new
                            {
                                dlCode = memberInfo.DLcode,
                                dlName = memberInfo.DlName,
                                registerDate = memberInfo.RegisterDate?.ToString("dd/MM/yyyy")
                            }
                        });
                    else
                        return Json(new
                        {
                            success = false,
                            message = "ไม่พบรหัสสมาชิก"
                        });
                }

                return Json(new
                {
                    success = false,
                    message = "ไม่พบรหัสสมาชิก"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching member");
                return Json(new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการค้นหา"
                });
            }
        }

        /// <summary>
        /// API สำหรับค้นหารหัส Center
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchCenters(string centercode)
        {
            try
            {
                var token = GetToken();
                var passkey = GetPasskey();

                if (string.IsNullOrEmpty(centercode))
                {
                    return Json(new
                    {
                        success = false,
                        message = "กรุณาระบุรหัส Center"
                    });
                }

                var centers = await _orderService.FindCenterFromApi(token, passkey, centercode);

                if (centers != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = centers
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "ไม่พบรหัส Center"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching member");
                return Json(new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการค้นหา"
                });
            }
        }
        #endregion

        #region Cart APIs

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var token = GetToken();
                var passkey = _config["Api:Passkey"];

                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "ไม่พบ Token" });

                var result = await _cartService.GetCartAsync(token, passkey);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        items = result.Data.Items,
                        itemCount = result.Data.Items.Sum(x=>x.Quantity),
                        totalPrice = result.Data.TotalAmount,
                        totalPV = result.Data.TotalPV,
                        totalBV = result.Data.TotalBV,
                        shippingFee = result.Data.ShippingFee,
                        memberCode = result.Data.MemberCode,
                        centerCode = result.Data.CenterCode,
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(
            [FromForm] string productId,
            [FromForm] int quantity,
            [FromForm] string dlCode = null,           // ⭐ NEW
            [FromForm] string dlName = null,           // ⭐ NEW
            [FromForm] string registerDate = null,     // ⭐ NEW
            [FromForm] string centerCode = null,       // ⭐ NEW
            [FromForm] string centerName = null)       // ⭐ NEW
        {
            try
            {
                var token = GetToken();
                var passkey = _config["Api:Passkey"];
                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "ไม่พบ Token" });
                if (string.IsNullOrEmpty(productId) || quantity <= 0)
                    return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });

                // ⭐ Validate DL และ Center
                if (string.IsNullOrEmpty(dlCode))
                    return Json(new { success = false, message = "กรุณาเลือกรหัสสมาชิก (DL) ก่อนเพิ่มสินค้า" });

                //if (string.IsNullOrEmpty(centerCode))
                //    return Json(new { success = false, message = "กรุณาเลือก Hybrid gold ก่อนเพิ่มสินค้า" });

                // 1. ดึง products จาก Session
                var productListJson = HttpContext.Session.GetString("ProductList");
                List<Product>? products = null;
                if (!string.IsNullOrEmpty(productListJson))
                {
                    products = JsonSerializer.Deserialize<List<Product>>(productListJson);
                }
                // ถ้าไม่มีใน Session ให้ดึงจาก API
                if (products == null || !products.Any())
                {
                    products = await GetProductListForTopupFromApi();
                    if (products != null && products.Any())
                    {
                        // เก็บไว้ใน Session
                        HttpContext.Session.SetString("ProductList", JsonSerializer.Serialize(products));
                    }
                }
                if (products == null || !products.Any())
                    return Json(new { success = false, message = "ไม่สามารถดึงข้อมูลสินค้าได้" });

                // 2. หา product ที่ต้องการ
                var product = products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null)
                    return Json(new { success = false, message = "ไม่พบสินค้า" });

                //// 3. ตรวจสอบ stock
                //if (product.StockQuantity < quantity || limit  )
                //    return Json(new { success = false, message = "สินค้าไม่เพียงพอ" });

                // 4. สร้าง AddToCartRequest
                var userdata = CookieHelper.GetCookie(_httpContextAccessor, CookieHelper.UserInfoKey);
                var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(userdata);
                var addToCartRequest = new AddToCartRequest
                {
                    ProductID = product.ProductId,
                    ProductName = product.ProductName,
                    ProductImage = product.ImageUrl ?? "",
                    Price = product.Price,
                    PV = product.PV,
                    Quantity = quantity,
                    Makerby = responseData?.memberInfo[0].membercode,

                    // ⭐ เพิ่ม DL และ Center
                    DLCode = dlCode,
                    DLName = dlName,
                    RegisterDate = string.IsNullOrEmpty(registerDate) ? null : DateTime.TryParse(registerDate, out var dt) ? dt : (DateTime?)null,
                    CenterCode = centerCode,
                    CenterName = centerName
                };

                // 5. เรียก API เพิ่มลง Cart
                var result = await _cartService.AddToCartAsync(token, passkey, addToCartRequest);
                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message ?? "เพิ่มสินค้าลงตะกร้าแล้ว",
                        cartCount = result.Data?.ItemCount ?? 0
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "ไม่สามารถเพิ่มสินค้าได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในระบบ" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart([FromForm] string productId, [FromForm] int quantity)
        {
            try
            {
                var token = GetToken();
                var passkey = _config["Api:Passkey"];

                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "ไม่พบ Token" });

                var updateRequest = new UpdateCartRequest
                {
                    ProductID = productId,
                    Quantity = quantity
                };

                var result = await _cartService.UpdateCartAsync(token, passkey, updateRequest);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "อัพเดทสินค้าแล้ว",
                        cartCount = result.Data?.ItemCount ?? 0
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromForm] string productId)
        {
            try
            {
                var token = GetToken();
                var passkey = _config["Api:Passkey"];

                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "ไม่พบ Token" });

                var result = await _cartService.RemoveFromCartAsync(token, passkey, productId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "ลบสินค้าออกจากตะกร้าแล้ว",
                        cartCount = result.Data?.ItemCount ?? 0
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var token = GetToken();
                var passkey = _config["Api:Passkey"];

                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "ไม่พบ Token" });

                var result = await _cartService.ClearCartAsync(token, passkey);

                if (result.Success)
                {
                    return Json(new { success = true, message = "ล้างตะกร้าเรียบร้อยแล้ว" });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder()
        {
            try
            {
                var token = GetToken();
                var passkey = _config["Api:Passkey"];

                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "ไม่พบ Token" });

                var result = await _cartService.CheckoutAsync(token, passkey);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "สร้างคำสั่งซื้อเรียบร้อยแล้ว",
                        orderId = result.OrderID,
                        amount = result.TotalAmount
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                return Json(new { success = false, message = "เกิดข้อผิดพลาด" });
            }
        }

        #endregion

        #region Helper Classes

        public class ApiResponse<T>
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("data")]
            public T? Data { get; set; }
        }
        public class BankSlipRequest
        {
            public string OrderID { get; set; }
            public List<string> Slips { get; set; } // รายการของ Base64 strings
        }
        #endregion
    }
}