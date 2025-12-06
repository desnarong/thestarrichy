using Microsoft.VisualBasic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    public interface ICartService
    {
        Task<CartData> GetCartAsync(string memberCode);
        Task<int> AddToCartAsync(string memberCode, AddToCartRequest request);
        Task<bool> UpdateCartAsync(string memberCode, UpdateCartRequest request);
        Task<bool> RemoveFromCartAsync(string memberCode, string productId);
        Task<bool> ClearCartAsync(string memberCode);
        Task<string> CheckoutAsync(string memberCode);
        Task<bool> UpdateCartDLCenterAsync(string memberCode, UpdateCartDLCenterRequest request);
        Task<bool> ValidatePasskeyAsync();
    }

    public class CartService : ICartService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartService> _logger;

        public CartService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CartService> logger)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private async Task<string> GetPasskeyAsync(string column)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string password = "";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting passkey");
            }

            return password;
        }

        public async Task<bool> ValidatePasskeyAsync()
        {
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return false;
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            return passkey == passwordEncode1 || passkey == passwordEncode2;
        }

        public async Task<CartData> GetCartAsync(string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            var cartData = new CartData();

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // เรียกใช้ SP_GetMemberCart
                    using (var command = new SqlCommand("SP_GetMemberCart", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MemberCode", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Result Set 1: ข้อมูลตะกร้า
                            if (await reader.ReadAsync())
                            {
                                // ตรงกับ SP_GetMemberCart ที่แก้ไขแล้ว
                                cartData.CartID = reader.GetDecimal(0);                // CartID
                                cartData.MemberCode = reader.GetString(1);             // MemberCode
                                cartData.CreatedDate = reader.GetDateTime(2);          // CreatedDate
                                cartData.ExpiryDate = reader.GetDateTime(3);           // ExpiryDate
                                cartData.Status = reader.GetString(4);                 // Status
                                cartData.TotalAmount = reader.GetDecimal(5);           // TotalAmount
                                cartData.TotalPV = reader.GetDecimal(6);               // TotalPV
                                cartData.HoursRemaining = reader.IsDBNull(7) ? 0 : reader.GetInt32(7); // HoursRemaining

                                // ⭐ ฟิลด์ใหม่ (index 8-13)
                                cartData.CenterCode = reader.IsDBNull(8) ? null : reader.GetString(8);       // CenterCode (เดิม)
                                cartData.CenterName = reader.IsDBNull(9) ? null : reader.GetString(9);       // CenterName (NEW)
                                cartData.Makerby = reader.IsDBNull(10) ? null : reader.GetString(10);        // Makerby (เดิม)
                                cartData.DLCode = reader.IsDBNull(11) ? null : reader.GetString(11);         // DLCode (NEW)
                                cartData.DLName = reader.IsDBNull(12) ? null : reader.GetString(12);         // DLName (NEW)
                                cartData.RegisterDate = reader.IsDBNull(13) ? null : reader.GetDateTime(13); // RegisterDate (NEW)
                                cartData.ShippingFee = reader.GetDecimal(14);           // ShippingFee
                                cartData.BillType = reader.GetString(15);
                            }

                            // Result Set 2: รายการสินค้า
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    cartData.Items.Add(new CartItem
                                    {
                                        CartItemID = reader.GetInt32(0),
                                        ProductID = reader.GetString(2),
                                        ProductCode = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                        ProductName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                        ProductImage = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                        Price = reader.GetDecimal(6),
                                        PV = reader.GetDecimal(7),
                                        Quantity = reader.GetInt32(8),
                                        SubTotal = reader.GetDecimal(9)
                                    });
                                }
                            }

                            cartData.ItemCount = cartData.Items.Count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for member {MemberCode}", memberCode);
            }

            return cartData;
        }

        public async Task<int> AddToCartAsync(string memberCode, AddToCartRequest request)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            int cartId = 0;

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // เรียกใช้ SP_AddToCart
                    using (var command = new SqlCommand("SP_AddToCart", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MemberCode", memberCode);
                        command.Parameters.AddWithValue("@ProductID", request.ProductID);
                        command.Parameters.AddWithValue("@ProductCode", request.ProductCode ?? "");
                        command.Parameters.AddWithValue("@ProductName", request.ProductName ?? "");
                        command.Parameters.AddWithValue("@ProductImage", request.ProductImage ?? "");
                        command.Parameters.AddWithValue("@Price", request.Price);
                        command.Parameters.AddWithValue("@PV", request.PV); 
                        command.Parameters.AddWithValue("@Quantity", request.Quantity);
                        command.Parameters.AddWithValue("@MakerBy", request.Makerby);
                        command.Parameters.AddWithValue("@BillType", request.BillType);

                        // ⭐ พารามิเตอร์ใหม่ (ใช้ CenterCode แทน CenterID)
                        command.Parameters.AddWithValue("@DLCode", (object)request.DLCode ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DLName", (object)request.DLName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RegisterDate", (object)request.RegisterDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CenterCode", (object)request.CenterCode ?? DBNull.Value);  // ⭐ CenterCode
                        command.Parameters.AddWithValue("@CenterName", (object)request.CenterName ?? DBNull.Value);

                        // SP จะ return CartID
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                cartId = reader.GetInt32(0);
                            }
                        }
                    }
                }

                // Log
                _logger.LogInformation(
                    "Added product {ProductID} to cart for member {MemberCode}. DL: {DLCode}, Center: {CenterCode}",
                    request.ProductID, memberCode, request.DLCode, request.CenterCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart for member {MemberCode}", memberCode);
                throw;
            }

            return cartId;
        }

        public async Task<bool> UpdateCartAsync(string memberCode, UpdateCartRequest request)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    using (var cmd = new SqlCommand("SP_UpdateCart", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // เพิ่ม Parameters
                        cmd.Parameters.AddWithValue("@MemberCode", memberCode);
                        cmd.Parameters.AddWithValue("@ProductID", request.ProductID);
                        cmd.Parameters.AddWithValue("@Quantity", request.Quantity);

                        // Execute และรับผลลัพธ์
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int success = reader.GetInt32(reader.GetOrdinal("Success"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (success == 1)
                                {
                                    // สามารถดึงข้อมูลเพิ่มเติมได้ถ้าต้องการ
                                    var cartId = reader.GetInt32(reader.GetOrdinal("CartID"));
                                    var totalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                                    var totalPV = reader.GetDecimal(reader.GetOrdinal("TotalPV"));
                                    var shippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee"));

                                    _logger.LogInformation(
                                        "Cart updated: CartID={CartID}, Total={Total}, PV={PV}, Shipping={Shipping}",
                                        cartId, totalAmount, totalPV, shippingFee);

                                    return true;
                                }
                                else
                                {
                                    _logger.LogWarning("Cart update failed: {Message}", message);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart for member {MemberCode}", memberCode);
            }

            return false;
        }

        public async Task<bool> RemoveFromCartAsync(string memberCode, string productId)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // เรียกใช้ SP_RemoveFromCart
                    using (var command = new SqlCommand("SP_RemoveFromCart", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MemberCode", memberCode);
                        command.Parameters.AddWithValue("@ProductID", productId);

                        await command.ExecuteNonQueryAsync();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart for member {MemberCode}", memberCode);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // เรียกใช้ SP_ClearCart
                    using (var command = new SqlCommand("SP_ClearCart", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MemberCode", memberCode);

                        await command.ExecuteNonQueryAsync();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for member {MemberCode}", memberCode);
                return false;
            }
        }

        public async Task<string> CheckoutAsync(string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string orderId = null;

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // เรียกใช้ SP_CompleteOrder
                    using (var command = new SqlCommand("SP_CompleteOrder", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MemberCode", memberCode);

                        // Output parameter สำหรับ OrderID
                        var outputParam = new SqlParameter("@OrderID", SqlDbType.NVarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputParam);

                        // Return value
                        var returnParam = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
                        returnParam.Direction = ParameterDirection.ReturnValue;

                        await command.ExecuteNonQueryAsync();

                        int returnValue = (int)returnParam.Value;

                        if (returnValue == 1)
                        {
                            orderId = outputParam.Value?.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checkout for member {MemberCode}", memberCode);
            }

            return orderId;
        }

        // ⭐ อัพเดท DL และ Center
        public async Task<bool> UpdateCartDLCenterAsync(string memberCode, UpdateCartDLCenterRequest request)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // เรียกใช้ SP_UpdateCartDLCenter
                    using (var command = new SqlCommand("SP_UpdateCartDLCenter", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MemberCode", memberCode);
                        command.Parameters.AddWithValue("@DLCode", (object)request.DLCode ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DLName", (object)request.DLName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RegisterDate", (object)request.RegisterDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CenterCode", (object)request.CenterCode ?? DBNull.Value);  // ⭐ CenterCode
                        command.Parameters.AddWithValue("@CenterName", (object)request.CenterName ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }

                    _logger.LogInformation("Updated cart DL/Center for member {MemberCode}", memberCode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart DL/Center for member {MemberCode}", memberCode);
                return false;
            }
        }
    }
}