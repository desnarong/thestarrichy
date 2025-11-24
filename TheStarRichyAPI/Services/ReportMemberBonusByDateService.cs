using System.Data.SqlClient;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace TheStarRichyApi.Services
{
    public interface IReportMemberBonusByDateService
    {
        Task<List<dynamic>> GetDisplayAsync();
    }
    public class ReportMemberBonusByDateService : IReportMemberBonusByDateService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportMemberBonusByDateService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> GetPermissionAsync(string column, string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string MemberPermission = "";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = $"SELECT {column}  from M06_permission where M06_PX1=@Membercode";

                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (!reader.IsDBNull(0))
                                    {
                                        MemberPermission = reader.GetString(0);
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

            return MemberPermission;
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
        public async Task<List<dynamic>> GetDisplayAsync()
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            // Get Membercode from JWT
            string memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberCode))
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    string Memberpermission = await GetPermissionAsync("M16", memberCode);

                    string query = "SELECT  Membercode,  Startdate, Enddate,  PaymentDate  , TypeofBonus ,SponserBonus,RebateBonus,WeakBonus";
                    query += ", MatchingBonus ,StrongBonus, AllSaleBonus, MobileBonus,  UnilevelBonus, SpecialBonus,  TotalBonus,  WTHTax,  OtherExpense ";
                    query += ",ServicePrice ,Coupon ,  BonusatferTax,Coupon ,Remark ,NetBonus, PeriodName ";
                    query += " FROM [000_Member_bonus_for_mobile]  (nolock) ";
 
                    query += " where Membercode = @Membercode";
                    query += " ORDER BY MemberCalc1 desc ,TypeofBonus desc ";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // Create a dynamic object (ExpandoObject) to store row data
                                dynamic row = new System.Dynamic.ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;

                                // Read each column dynamically
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.GetValue(i);
                                    rowDict[columnName] = columnValue;
                                }

                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<dynamic> { new { Membercode = "", Error = "An error occurred while fetching data" } };
            }

            return result.Count > 0 ? result : new List<dynamic> { new { Membercode = "" } };
        }
    }
}
