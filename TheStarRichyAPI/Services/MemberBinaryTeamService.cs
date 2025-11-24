using System.Data.SqlClient;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace TheStarRichyApi.Services
{
    public interface IMemberBinaryTeamService
    {
        Task<List<dynamic>> GetDisplayAsync();
    }
    public class MemberBinaryTeamService : IMemberBinaryTeamService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberBinaryTeamService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

                    //string Memberpermission = await GetPermissionAsync("M16", memberCode);


                    string query = "SELECT Membercode,PositionLevel,ChildCode,Membername,Sponsername,Memberposition,MemberpositionName,MemberpositionRanking";
                    query += ", MemberpositionRankingName, PersonalPV, LeftCountActive, RightCountActive, LeftBal, Rightbal, TotalBalance, CurrentLeftPV, CurrentRightPV";
                    query += ", BWDLeftPV, BWDRightPV, NewLeft, NewRight, Maxto2, TName1, TName2, EName1, EName2, Travelpoint1, travelpoint2";
                    query += ", CurrentMonthQualifyPV, LastMonthQualifyPV, LastMonthQualifyStatus, CurrentMonthQualifyStatus, FirstQdate";
                    query += ", CurrentMonth, NextCMonth, CurrentMonth1, LastCMonth, MemberPostionPicture";             
                    query += " FROM [000_Member_Binary_Team]  (nolock) ";

                 
                    query += " where   Membercode = @Membercode";
                     

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
