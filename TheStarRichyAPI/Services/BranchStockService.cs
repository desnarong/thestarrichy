using System.Data.SqlClient;
using System.Dynamic;
using System.Text.Json;

namespace TheStarRichyApi.Services
{
    public interface IBranchStockService
    {
        /// <summary>
        /// ดึงรายการสินค้าจาก Branchcode (Function 1)
        /// </summary>
        Task<List<dynamic>> GetStockByBranchAsync(string branchCode);

        /// <summary>
        /// ตรวจสอบว่าสินค้าทั้งหมดใน array มีใน Branchcode นี้หรือไม่ (Function 2)
        /// ส่ง ProductCode array + Branchcode ไปเช็ค
        /// ต้องเจอทั้งหมดใน array ถึงจะตอบกลับมาเป็น true ไม่เช่นนั้นเป็น false
        /// </summary>
        Task<bool> CheckStockByBranchAsync(string branchCode, List<string> productCodes);
    }

    public class BranchStockService : IBranchStockService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BranchStockService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetPasskeyAsync(string column)
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
            catch (Exception)
            {
                // Log exception in production
            }

            return password;
        }

        private bool ValidatePasskey(string passkey)
        {
            if (string.IsNullOrEmpty(passkey))
                return false;

            string passwordEncode1 = GetPasskeyAsync("Passkey1").GetAwaiter().GetResult();
            string passwordEncode2 = GetPasskeyAsync("Passkey2").GetAwaiter().GetResult();

            return passkey == passwordEncode1 || passkey == passwordEncode2;
        }

        /// <summary>
        /// Function 1: ดึงข้อมูลจาก [000_checkbranchstock] ตาม Branchcode
        /// ตอบกลับมาเป็น List สินค้า (ProductCode, ProductName)
        /// </summary>
        public async Task<List<dynamic>> GetStockByBranchAsync(string branchCode)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey) || !ValidatePasskey(passkey))
            {
                return new List<dynamic>();
            }

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = @"SELECT ProductCode, ProductName, Branchcode 
                                     FROM [000_checkbranchstock] 
                                     WHERE Branchcode = @BranchCode";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@BranchCode", branchCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic row = new ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    rowDict[columnName] = columnValue;
                                }

                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new List<dynamic>();
            }

            return result.Count > 0 ? result : new List<dynamic>();
        }

        /// <summary>
        /// Function 2: ส่ง ProductCode เป็น array พร้อม Branchcode ไปเช็ค
        /// ต้องเจอทั้งหมดใน array ถึงจะตอบกลับมาเป็น true ไม่เช่นนั้นเป็น false
        /// </summary>
        public async Task<bool> CheckStockByBranchAsync(string branchCode, List<string> productCodes)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey) || !ValidatePasskey(passkey))
            {
                return false;
            }

            if (productCodes == null || productCodes.Count == 0)
            {
                return false;
            }

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // สร้าง parameter list สำหรับ IN clause
                    var parameters = new List<string>();
                    var sqlParams = new List<SqlParameter>();

                    for (int i = 0; i < productCodes.Count; i++)
                    {
                        string paramName = $"@ProductCode{i}";
                        parameters.Add(paramName);
                        sqlParams.Add(new SqlParameter(paramName, productCodes[i]));
                    }

                    string productCodeList = string.Join(", ", parameters);

                    string query = $@"
                        SELECT COUNT(DISTINCT ProductCode) AS FoundCount
                        FROM [000_checkbranchstock] 
                        WHERE Branchcode = @BranchCode 
                          AND ProductCode IN ({productCodeList})";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@BranchCode", branchCode);
                        command.Parameters.AddRange(sqlParams.ToArray());

                        var resultObj = await command.ExecuteScalarAsync();
                        int foundCount = resultObj != null ? Convert.ToInt32(resultObj) : 0;

                        // ต้องเจอทั้งหมดใน array ถึงจะ return true
                        return foundCount == productCodes.Count;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
