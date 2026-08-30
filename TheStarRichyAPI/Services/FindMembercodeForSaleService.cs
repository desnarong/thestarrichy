using System.Data.SqlClient;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace TheStarRichyApi.Services
{
    public interface IFindMembercodeForSaleService
    {
        Task<List<dynamic>> GetDisplayAsync(string referrerCode, string? uplineCode);
    }
    public class FindMembercodeForSaleService : IFindMembercodeForSaleService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FindMembercodeForSaleService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> GetPermissionAsync(string column, string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string MemberPermission = "N";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = $"SELECT {column}  from M06_permission where Membercode=@Membercode";

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
        private string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return "";

            if (DateTime.TryParse(dateObj.ToString(), out DateTime date))
            {
                return date.ToString("dd/MM/yyyy");
            }
            return "";
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
        public async Task<List<dynamic>> GetDisplayAsync(string referrerCode, string? uplineCode)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new List<dynamic>();
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new List<dynamic>();
            }

            // Get Membercode from JWT
            string memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(memberCode))
            {
                return new List<dynamic>();
            }

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    string query = "SELECT  top 1 aa.Membercode, aa.DLcode,aa.DlName,M06_X4 as RegisterDate,M06_X59 as Position ";
                    query += " FROM [000_Member_Binary_LeftRight_Search] aa (nolock) join M06 on aa.DLcode=M06_PX1";

                    //if (Memberpermission != "Y")
                    //{
                    //    query += "  join [000_Member_SponserTeam] bb  (nolock) on bb.Membercode=aa.Membercode and bb.DLCode=aa.MemberLeftCode  ";
                    //}

                    if (string.IsNullOrWhiteSpace(uplineCode))
                    {
                        query += " where aa.Membercode = @Membercode and aa.DLcode = @SearchDLcode";
                    }
                    else
                    {
                        query += " where aa.Membercode = @ReferrerCode and aa.DLcode = @UplineCode";
                    }



                    using (var command = new SqlCommand(query, con))
                    {
                        if (string.IsNullOrWhiteSpace(uplineCode))
                        {
                            command.Parameters.AddWithValue("@Membercode", memberCode);
                            command.Parameters.AddWithValue("@SearchDLcode", referrerCode);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@ReferrerCode", referrerCode);
                            command.Parameters.AddWithValue("@UplineCode", uplineCode);
                        }

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
                                    object columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    //RegisterDate = FormatDate(reader["RegisterDate"]);
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
                return new List<dynamic>();
            }

            return result.Count > 0 ? result : new List<dynamic>();
        }
    }
}
