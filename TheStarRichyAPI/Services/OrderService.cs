using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using TheStarRichyApi.Controllers;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    public interface IOrderService
    {
        Task<bool> ValidatePasskeyAsync();
        Task<CheckoutInfoResponse> SaveCheckoutInfoAsync(string memberCode, CheckoutInfoRequest request);
        Task<OrderSummary> GetOrderSummaryAsync(string memberCode, string orderID);
        Task<PaymentResponse> CreatePaymentAsync(string memberCode, PaymentRequest request);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentID);
        Task<OrderResult> ConfirmOrderAsync(string memberCode, string orderID);
        Task<bool> BankSlipOrderAsync(string memberCode, string orderID, string slips);
    }

    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;
        private readonly IKbankQrPaymentService _kbankService;
        private readonly string _connectionString;

        public OrderService(
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger,
            IKbankQrPaymentService kbankService)
        {
            _configuration = config;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _kbankService = kbankService;
            _connectionString = _configuration.GetConnectionString("MLMConnectionString");
        }

        #region Passkey Validation

        public async Task<string> GetPasskeyAsync(string column)
        {
            string password = "";

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = $"SELECT {column} FROM S02";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (!reader.IsDBNull(0))
                                    {
                                        password = reader.GetString(0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception in production
            }

            return password;
        }

        public async Task<bool> ValidatePasskeyAsync()
        {
            try
            {
                var passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
                if (string.IsNullOrEmpty(passkey))
                {
                    return false;
                }

                string passwordEncode1 = await GetPasskeyAsync("Passkey1");
                string passwordEncode2 = await GetPasskeyAsync("Passkey2");

                return !(passkey != passwordEncode1 && passkey != passwordEncode2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating passkey");
                return false;
            }
        }

        #endregion

        #region Save Checkout Info

        /// <summary>
        /// บันทึกข้อมูล Checkout Info โดยอัพเดท ShoppingOrder ที่มีอยู่แล้ว
        /// ไม่ใช้ SP แต่ใช้ SQL โดยตรง
        /// </summary>
        public async Task<CheckoutInfoResponse> SaveCheckoutInfoAsync(string memberCode, CheckoutInfoRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // ตรวจสอบชื่อ Stored Procedure ให้ตรงกับใน Database
                    using (var command = new SqlCommand("SP_UpdateShoppingOrder", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // 1. Parameter พื้นฐาน
                        command.Parameters.AddWithValue("@OrderID", request.OrderID);
                        command.Parameters.AddWithValue("@MemberCode", memberCode);
                        command.Parameters.AddWithValue("@PaymentMethod", (object)request.PaymentMethod ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliveryMethod", (object)request.DeliveryMethod ?? DBNull.Value);

                        // แปลง Bool เป็น Int (1/0)
                        command.Parameters.AddWithValue("@SendBill", request.SendInvoice);
                        command.Parameters.AddWithValue("@BranchCode", (object)request.BranchCode?.ToString() ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Remark", ""); // หรือรับค่า Remark จาก request

                        // ตัวแปรสำหรับเก็บค่าที่จะส่งเข้า SP
                        object deliveryAddr = DBNull.Value;
                        object province = DBNull.Value;
                        object district = DBNull.Value;
                        object subDistrict = DBNull.Value;
                        object postalCode = DBNull.Value;
                        object phone = DBNull.Value;
                        object recipient = DBNull.Value;
                        object initials = DBNull.Value;
                        int freqAddr = 0; // Default 0

                        // 2. Logic การเลือกที่อยู่ (ยุบรวม Logic เพื่อความสะอาดของโค้ด)
                        if (request.CustomAddress != null)
                        {
                            deliveryAddr = request.CustomAddress.AddressLine;
                            province = request.CustomAddress.Province;       // ส่งเป็น Code หรือ Name ตามที่ SP รองรับ
                            district = request.CustomAddress.District;
                            subDistrict = request.CustomAddress.SubDistrict;
                            postalCode = request.CustomAddress.PostalCode;
                            phone = request.CustomAddress.Phone;
                            recipient = request.CustomAddress.RecipientName;
                            initials = request.CustomAddress.Initials;
                            // แปลง Bool เป็น Int
                            freqAddr = request.CustomAddress.FavoriteAddress ?? 0;
                        }
                        else if (request.MemberAddress != null)
                        {
                            deliveryAddr = request.MemberAddress.AddressLine;
                            province = request.MemberAddress.Province;
                            district = request.MemberAddress.District;
                            subDistrict = request.MemberAddress.SubDistrict;
                            postalCode = request.MemberAddress.PostalCode;
                            phone = request.MemberAddress.Phone;
                            // กรณีใช้ที่อยู่เดิม ไม่ต้องใส่ชื่อผู้รับใหม่ (หรือจะใส่ก็ได้แล้วแต่ Requirement)
                        }
                        else if (request.MemberFavorite != null)
                        {
                            // ** ระวัง Mapping ตรงนี้ **
                            deliveryAddr = request.MemberFavorite.Contact_Other ?? ""; // หรือ Contact_Other
                            subDistrict = request.MemberFavorite.tambon ?? request.MemberFavorite.Contact_SubDistrict ?? ""; // อันนี้มักเป็น INT
                            district = request.MemberFavorite.amphoe ?? request.MemberFavorite.Contact_District ?? "";
                            province = request.MemberFavorite.province ?? request.MemberFavorite.Contact_Province ?? "";
                            postalCode = request.MemberFavorite.zipcode ?? request.MemberFavorite.Contact_Zipcode ?? "";
                            phone = request.MemberFavorite.Contact_Phone ?? "";
                        }

                        // 3. Add Parameters ที่อยู่
                        command.Parameters.AddWithValue("@DeliveryAddress", deliveryAddr ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliveryProvince", province ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliveryDistrict", district ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliverySubDistrict", subDistrict ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliveryPostalCode", postalCode ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliveryPhone", phone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RecipientName", recipient ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Initials", initials ?? DBNull.Value);

                        // ** สำคัญ: ชื่อต้องตรง SQL (@Frequentlyaddress) **
                        command.Parameters.AddWithValue("@FavoriteAddress", freqAddr);

                        // Parameter ModifiedBy
                        command.Parameters.AddWithValue("@ModifiedBy", memberCode);

                        // 4. Execute และรับค่า Return (แก้ไขจาก ReturnValue เป็น ExecuteScalar)
                        // เพราะ SQL จบด้วย SELECT @OrderID 

                        var resultObj = await command.ExecuteScalarAsync();
                        string resultOrderID = resultObj != null ? resultObj.ToString() : "";

                        return new CheckoutInfoResponse
                        {
                            Success = !string.IsNullOrEmpty(resultOrderID),
                            Message = "บันทึกข้อมูลสำเร็จ",
                            OrderID = resultOrderID
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving checkout info");
                return new CheckoutInfoResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาด: " + ex.Message
                };
            }
        }

        #endregion

        #region Get Order Summary

        /// <summary>
        /// ดึงสรุปคำสั่งซื้อ - ใช้ SP_GetOrderDetails ที่มีอยู่แล้ว
        /// </summary>
        public async Task<OrderSummary> GetOrderSummaryAsync(string memberCode, string orderID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SP_GetOrderDetails", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@OrderID", orderID);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            OrderSummary summary = null;

                            // Result Set 1: Order Header
                            if (await reader.ReadAsync())
                            {
                                summary = new OrderSummary
                                {
                                    OrderID = reader["OrderID"].ToString(),
                                    OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                    MemberCode = reader["MemberCode"].ToString(),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                    TotalPV = Convert.ToDecimal(reader["TotalPV"]),
                                    ShippingFee = Convert.ToDecimal(reader["ShippingFee"]),
                                    GrandTotal = reader["GrandTotal"] != DBNull.Value ? Convert.ToDecimal(reader["GrandTotal"]) : 0,
                                    PaymentMethod = reader["PaymentMethod"]?.ToString(),
                                    DeliveryMethod = reader["DeliveryMethod"]?.ToString(),
                                    Status = reader["Status"]?.ToString(),
                                    SendInvoice = Convert.ToInt16(reader["SendBill"]),
                                    BillNo = reader["BillNo"]?.ToString(),
                                    BillType = reader["BillType"]?.ToString(),
                                    Items = new List<OrderItem>(),
                                    DeliveryInfo = new DeliveryInfo
                                    {
                                        FullAddress = reader["DeliveryAddress"]?.ToString(),
                                        Province = reader["DeliveryProvince"]?.ToString(),
                                        District = reader["DeliveryDistrict"]?.ToString(),
                                        SubDistrict = reader["DeliverySubDistrict"]?.ToString(),
                                        PostalCode = reader["DeliveryPostalCode"]?.ToString(),
                                        PhoneNumber = reader["DeliveryPhone"]?.ToString()
                                    }
                                };
                            }

                            // Result Set 2: Order Items
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    summary?.Items.Add(new OrderItem
                                    {
                                        ProductID = reader["ProductID"].ToString(),
                                        ProductCode = reader["ProductCode"]?.ToString(),
                                        ProductName = reader["ProductName"].ToString(),
                                        Quantity = Convert.ToInt32(reader["Quantity"]),
                                        Price = Convert.ToDecimal(reader["Price"]),
                                        PV = Convert.ToDecimal(reader["PV"]),
                                        TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                                        TotalPV = Convert.ToDecimal(reader["TotalPV"])
                                    });
                                }
                            }

                            return summary;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order summary");
                return null;
            }
        }

        #endregion

        #region Create Payment

        /// <summary>
        /// สร้างการชำระเงิน - เรียก KBank API สร้าง QR Code
        /// </summary>
        public async Task<PaymentResponse> CreatePaymentAsync(string memberCode, PaymentRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 1. ดึงข้อมูล Order
                    string selectQuery = @"
                        SELECT OrderID, TotalAmount, GrandTotal, MemberCode
                        FROM ShoppingOrder
                        WHERE OrderID = @OrderID AND MemberCode = @MemberCode;
                    ";

                    decimal amount = 0;
                    string orderID = null;

                    using (var command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", request.OrderID);
                        command.Parameters.AddWithValue("@MemberCode", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                orderID = reader["OrderID"].ToString();
                                amount = reader["GrandTotal"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["GrandTotal"])
                                    : Convert.ToDecimal(reader["TotalAmount"]);
                            }
                            else
                            {
                                return new PaymentResponse
                                {
                                    Success = false,
                                    Message = "ไม่พบคำสั่งซื้อ"
                                };
                            }
                        }
                    }

                    // 2. เรียก KBank API สร้าง QR Code (เฉพาะ PromptPay)
                    string qrCodeData = null;
                    string paymentUrl = null;
                    string partnerTxnUid = null;

                    if (request.PaymentMethod == "PromptPay")
                    {
                        try
                        {
                            var qrRequest = new Models.Kbank.QrPaymentRequest
                            {
                                TxnAmount = amount,
                                Reference1 = orderID,
                                Reference2 = memberCode,
                                Reference3 = DateTime.Now.ToString("yyyyMMdd")
                            };

                            var qrResponse = await _kbankService.CreateQrPaymentAsync(qrRequest);

                            if (qrResponse != null && !string.IsNullOrEmpty(qrResponse.QrCode))
                            {
                                qrCodeData = qrResponse.QrCode;
                                partnerTxnUid = qrResponse.PartnerTxnUid;

                                _logger.LogInformation(
                                    "KBank QR Created - OrderID: {OrderID}, PartnerTxnUid: {PartnerTxnUid}, Amount: {Amount}",
                                    orderID, partnerTxnUid, amount);
                            }
                            else
                            {
                                _logger.LogWarning("KBank QR response is null or empty");
                            }
                        }
                        catch (Exception kbankEx)
                        {
                            _logger.LogError(kbankEx, "Error calling KBank API");
                            // Continue anyway - จะใช้วิธีอื่นชำระเงินได้
                        }
                    }
                    else if (request.PaymentMethod == "CreditCard")
                    {
                        // TODO: Implement Credit Card Payment URL
                        paymentUrl = "/payment/creditcard"; // Placeholder
                    }

                    // 3. สร้าง PaymentID และบันทึกลง Database
                    string paymentID = partnerTxnUid ?? ("PAY" + DateTime.Now.ToString("yyyyMMddHHmmss"));

                    string updateQuery = @"
                        UPDATE ShoppingOrder
                        SET PaymentID = @PaymentID,
                            PaymentMethod = @PaymentMethod,
                            PaymentReferenceNo = @PaymentReferenceNo,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @MemberCode
                        WHERE OrderID = @OrderID AND MemberCode = @MemberCode;
                    ";

                    using (var command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentID", paymentID);
                        command.Parameters.AddWithValue("@PaymentMethod", request.PaymentMethod);
                        command.Parameters.AddWithValue("@PaymentReferenceNo", partnerTxnUid ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@OrderID", request.OrderID);
                        command.Parameters.AddWithValue("@MemberCode", memberCode);

                        await command.ExecuteNonQueryAsync();
                    }

                    // 4. Return Response
                    return new PaymentResponse
                    {
                        Success = true,
                        Message = "สร้างการชำระเงินสำเร็จ",
                        Data = new PaymentData()
                        {
                            PaymentID = paymentID,
                            Amount = amount,
                            QRCode = qrCodeData,  // ✅ เปลี่ยนเป็น QRCode
                            OrderID = orderID,
                            GatewayTransactionID = partnerTxnUid
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return new PaymentResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาด: " + ex.Message
                };
            }
        }

        #endregion

        #region Get Payment Status

        /// <summary>
        /// ตรวจสอบสถานะการชำระเงิน - Query จาก ShoppingOrder
        /// </summary>
        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT PaymentID, PaymentStatus, PaidDate, GrandTotal
                        FROM ShoppingOrder
                        WHERE PaymentID = @PaymentID;
                    ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentID", paymentID);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new PaymentStatusResponse
                                {
                                    PaymentID = reader["PaymentID"].ToString(),
                                    Status = reader["PaymentStatus"]?.ToString() ?? "Pending",
                                    PaidDate = reader["PaidDate"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["PaidDate"])
                                        : (DateTime?)null,
                                    Amount = reader["GrandTotal"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["GrandTotal"])
                                        : 0
                                };
                            }
                        }
                    }
                }

                return new PaymentStatusResponse
                {
                    PaymentID = paymentID,
                    Status = "NotFound",
                    Amount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return new PaymentStatusResponse
                {
                    PaymentID = paymentID,
                    Status = "Error",
                    Amount = 0
                };
            }
        }

        #endregion

        #region Confirm Order

        /// <summary>
        /// ยืนยันคำสั่งซื้อ - ใช้ SP_UpdatePaymentStatus
        /// </summary>
        public async Task<OrderResult> ConfirmOrderAsync(string memberCode, string orderID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SP_UpdatePaymentStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@OrderID", orderID);
                        command.Parameters.AddWithValue("@PaymentStatus", "Paid");
                        command.Parameters.AddWithValue("@PaymentReferenceNo", (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UpdatedBy", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int success = reader["Success"] != DBNull.Value ? Convert.ToInt32(reader["Success"]) : 0;

                                if (success == 1)
                                {
                                    return new OrderResult
                                    {
                                        BillNo = reader["BillNo"] != DBNull.Value ? reader["BillNo"].ToString() : null
                                    };
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                return null;
            }
        }

        public async Task<bool> BankSlipOrderAsync(string memberCode, string orderID, string slips)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // ใช้ SP_UpdatePaymentStatus ที่มีอยู่แล้ว
                    using (var command = new SqlCommand("SP_UpdateOrderStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@OrderID", orderID);
                        command.Parameters.AddWithValue("@Status", "WaitVerify");
                        command.Parameters.AddWithValue("@Remarks", slips);
                        command.Parameters.AddWithValue("@UpdatedBy", memberCode);

                        // Return value
                        var returnParam = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
                        returnParam.Direction = ParameterDirection.ReturnValue;

                        await command.ExecuteNonQueryAsync();

                        int returnValue = (int)returnParam.Value;
                        return returnValue == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                return false;
            }
        }
        #endregion
    }

    #region Request/Response Models (ยังคงเหมือนเดิม)

    public class CheckoutInfoRequest
    {
        public string PaymentMethod { get; set; } // "PromptPay", "CreditCard"

        public string DeliveryMethod { get; set; } // "Delivery", "Pickup"

        public string DeliveryAddressType { get; set; } // "Member", "Card", "Other", "Branch"

        public int? AddressID { get; set; }

        public CustomAddressInfo? CustomAddress { get; set; }
        public MemberAddressData? MemberAddress { get; set; }
        public MemberFavoriteAddressData? MemberFavorite { get; set; }

        public int SendInvoice { get; set; }

        public string? BranchCode { get; set; } // ถ้าเลือก Pickup

        public string? BranchName { get; set; } = string.Empty;
        public string OrderID { get; set; }

    }

    public class MemberAddressData
    {
        public int? AddressID { get; set; }
        public string? AddressType { get; set; }
        public string? AddressTypeDisplay { get; set; }
        public string? AddressLine { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? FullAddress { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class MemberFavoriteAddressData
    {
        public int? FavoriteID { get; set; }
        public string? Membercode { get; set; }
        public string? ContacNickname { get; set; }
        public string? Contactperson { get; set; }
        public string? Contact_HouseNumber { get; set; }
        public string? Contact_Alley { get; set; }
        public string? Contact_Road { get; set; }
        public string? Contact_Building { get; set; }
        public string? Contact_Floor { get; set; }
        public string? Contact_Other { get; set; }
        public string? Contact_Zipcode { get; set; }
        public int? Contact_TAMBON_ID { get; set; }
        public string? Contact_Phone { get; set; }
        public string? Contact_District { get; set; }
        public string? Contact_SubDistrict { get; set; }
        public string? Contact_Province { get; set; }
        public string? Provincename { get; set; }   // M32_X2 as Provincename
        public string? Contact_Primary { get; set; }
        public string? tambon { get; set; }
        public string? amphoe { get; set; }
        public string? province { get; set; }
        public string? zipcode { get; set; }
    }

    public class CustomAddressInfo
    {
        public string? AddressLine { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? RecipientName { get; set; }
        public string? Initials { get; set; }
        public int? FavoriteAddress { get; set; }
    }

    public class CheckoutInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OrderID { get; set; }
    }

    public class OrderSummary
    {
        public string OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string? PaymentMethod { get; set; }
        public string? DeliveryMethod { get; set; }
        public DeliveryInfo? DeliveryInfo { get; set; }
        public string Status { get; set; }
        public int SendInvoice { get; set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
    }

    public class OrderItem
    {
        public string ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal PV { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPV { get; set; }
    }

    public class DeliveryInfo
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullAddress { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string? PostalCode { get; set; }
    }

    public class PaymentRequest
    {
        public string OrderID { get; set; }
        public string PaymentMethod { get; set; }
        public CreditCardInfo? CreditCard { get; set; }
    }

    public class CreditCardInfo
    {
        public string? CardNumber { get; set; }
        public string? CardName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public PaymentData Data { get; set; }
    }
    public class PaymentData
    {
        public string PaymentID { get; set; }
        public string OrderID { get; set; }
        public decimal Amount { get; set; }

        // สำหรับ PromptPay
        public string QRCode { get; set; }
        public string QRReference { get; set; }
        public DateTime? ExpiryDateTime { get; set; }

        // สำหรับบัตรเครดิต
        public string CardBrand { get; set; }
        public string Last4Digits { get; set; }
        public string GatewayTransactionID { get; set; }
    }
    public class PaymentStatusResponse
    {
        public string PaymentID { get; set; }
        public string Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class MemberAddress
    {
        public int? AddressID { get; set; }
        public string? AddressType { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullAddress { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
    }

    public class Branch
    {
        public int BranchID { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? Address { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OpeningHours { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion
}