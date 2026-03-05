using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data.SqlClient;
using System.Security.Claims;
using static TheStarRichyApi.Services.ReportMemberSponserTeamService;

namespace TheStarRichyApi.Services
{
    public interface IReportMemberSponserTeamService
    {
        Task<List<MemberSponsorDto>> GetDisplayAsync();
    }
    public class ReportMemberSponserTeamService : IReportMemberSponserTeamService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportMemberSponserTeamService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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
        public class MemberSponsorDto
        {
            public string Membercode { get; set; }
            public string LevelName { get; set; }
            public string DLCode { get; set; }
            public string DLName { get; set; }
            public DateTime? RegisterDate { get; set; }
            public DateTime? QualifyDate { get; set; }
            public string SIDE { get; set; }
            public string PositionName { get; set; }
            public string PrestigeRankingEngName { get; set; }
            public decimal? PersonalPV { get; set; }
            public string Sidex { get; set; }
            public decimal? PVLeft { get; set; }
            public decimal? PVRight { get; set; }
            public string SponserName { get; set; }
            public string Qualify { get; set; }
            public decimal? LastMonthQualifyPV { get; set; }
            public decimal? PresentMonthQualifyPV { get; set; }
            public int? LeftCountActive { get; set; }
            public int? RightCountActive { get; set; }
            public decimal? Travelpoint1 { get; set; }
            public decimal? Travelpoint2 { get; set; }
            public decimal? TotalBalance { get; set; }
            public decimal? TotalLeftBalance { get; set; }
            public decimal? TotalRightBalance { get; set; }
            public string NextPosition { get; set; }
            public decimal? NextPosaddLeftBalance { get; set; }
            public decimal? NextPosaddRightBalance { get; set; }
        }

        public async Task<List<MemberSponsorDto>> GetDisplayAsync()
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new List<MemberSponsorDto> { new MemberSponsorDto { Membercode = "" } };
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new List<MemberSponsorDto> { new MemberSponsorDto { Membercode = "" } };
            }

            // Get Membercode from JWT
            string memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberCode))
            {
                return new List<MemberSponsorDto> { new MemberSponsorDto { Membercode = "" } };
            }

            var result = new List<MemberSponsorDto>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    string query = @"SELECT aa.Membercode, aa.LevelName, aa.DLCode, aa.DLName, aa.RegisterDate,
                    aa.QualifyDate, aa.SIDE, aa.PositionName, aa.PrestigeRankingEngName, aa.PersonalPV, 
                    aa.Sidex, aa.PVLeft, aa.PVRight, aa.SponserName1 AS SponserName, aa.Qualify,
                    aa.LastMonthQualifyPV, aa.PresentMonthQualifyPV, aa.LeftCountActive, aa.RightCountActive, 
                    aa.Travelpoint1, aa.Travelpoint2, COALESCE(aa.TotalBalance, 0) as TotalBalance,
                    aa.TotalLeftBalance, aa.TotalRightBalance, aa.NextPosition, 
                    aa.NextPosaddLeftBalance, aa.NextPosaddRightBalance
                    FROM [000_Member_SponserTeam] aa (nolock) 
                    WHERE aa.Membercode = @Membercode
                    ORDER BY aa.DLCode";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new MemberSponsorDto
                                {
                                    Membercode = reader["Membercode"]?.ToString(),
                                    LevelName = reader["LevelName"]?.ToString(),
                                    DLCode = reader["DLCode"]?.ToString(),
                                    DLName = reader["DLName"]?.ToString(),
                                    RegisterDate = reader["RegisterDate"] as DateTime?,
                                    QualifyDate = reader["QualifyDate"] as DateTime?,
                                    SIDE = reader["SIDE"]?.ToString(),
                                    PositionName = reader["PositionName"]?.ToString(),
                                    PrestigeRankingEngName = reader["PrestigeRankingEngName"]?.ToString(),
                                    PersonalPV = reader["PersonalPV"] as decimal?,
                                    Sidex = reader["Sidex"]?.ToString(),
                                    PVLeft = reader["PVLeft"] as decimal?,
                                    PVRight = reader["PVRight"] as decimal?,
                                    SponserName = reader["SponserName"]?.ToString(),
                                    Qualify = reader["Qualify"]?.ToString(),
                                    LastMonthQualifyPV = reader["LastMonthQualifyPV"] as decimal?,
                                    PresentMonthQualifyPV = reader["PresentMonthQualifyPV"] as decimal?,
                                    LeftCountActive = reader["LeftCountActive"] as int?,
                                    RightCountActive = reader["RightCountActive"] as int?,
                                    Travelpoint1 = reader["Travelpoint1"] as decimal?,
                                    Travelpoint2 = reader["Travelpoint2"] as decimal?,
                                    TotalBalance = reader["TotalBalance"] as decimal?,
                                    TotalLeftBalance = reader["TotalLeftBalance"] as decimal?,
                                    TotalRightBalance = reader["TotalRightBalance"] as decimal?,
                                    NextPosition = reader["NextPosition"]?.ToString(),
                                    NextPosaddLeftBalance = reader["NextPosaddLeftBalance"] as decimal?,
                                    NextPosaddRightBalance = reader["NextPosaddRightBalance"] as decimal?
                                };

                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<MemberSponsorDto> { new MemberSponsorDto { Membercode = "", /* ไม่ต้องมี Error property */ } };
            }

            return result.Count > 0 ? result : new List<MemberSponsorDto> { new MemberSponsorDto { Membercode = "" } };
        }
    }
}
