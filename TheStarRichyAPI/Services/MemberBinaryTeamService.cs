using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TheStarRichyApi.Services
{
    public interface IMemberBinaryTeamService
    {
        Task<MemberBinaryTeamResponseDto> GetDisplayAsync(string? binaryCode = null, string direction = null);

        // แก้ไข Return Type จาก Task<string> เป็น Task<MemberBinaryTeamResponseDto>
        Task<MemberBinaryTeamResponseDto> GetExtremeBinaryPathAsync(string rootCode, string direction);
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

        public async Task<MemberBinaryTeamResponseDto> GetDisplayAsync(string? binaryCode = null, string direction = null)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            // Use provided memberCode or get from JWT
            var memberCode = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(memberCode))
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            MemberBinaryTeamResponseDto result = null;
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    bool isUnderDownline = false;
                    if (direction == null)
                    {
                        // เช็คว่า binaryCode อยู่ใต้ memberCode (JWT) หรือไม่
                        string checkDownlineQuery = @"
                        WITH PureTree AS (
                            SELECT MemberCode AS UplineCode, Member_1_1_L AS DownlineCode 
                            FROM [dbo].[MemberLevel] 
                            WHERE Member_1_1_L IS NOT NULL AND Member_1_1_L <> ''
        
                            UNION ALL
        
                            SELECT MemberCode AS UplineCode, Member_1_2_R AS DownlineCode 
                            FROM [dbo].[MemberLevel] 
                            WHERE Member_1_2_R IS NOT NULL AND Member_1_2_R <> ''
                        ),
                        UplineCTE AS (
                            SELECT UplineCode
                            FROM PureTree
                            WHERE DownlineCode = @DownlineCode
        
                            UNION ALL
        
                            SELECT t.UplineCode
                            FROM PureTree t
                            INNER JOIN UplineCTE c ON t.DownlineCode = c.UplineCode
                            WHERE c.UplineCode <> @UplineCode 
                        )
                        SELECT CASE WHEN EXISTS (SELECT 1 FROM UplineCTE WHERE UplineCode = @UplineCode) 
                               THEN 1 ELSE 0 END 
                        OPTION (MAXRECURSION 0);";

                        using (var cmd = new SqlCommand(checkDownlineQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@UplineCode", memberCode);
                            cmd.Parameters.AddWithValue("@DownlineCode", binaryCode ?? memberCode);

                            var _result = await cmd.ExecuteScalarAsync();

                            if (_result != null && _result != DBNull.Value)
                            {
                                isUnderDownline = Convert.ToInt32(_result) == 1;
                            }
                        }
                    }
                    else
                    {
                        isUnderDownline = true;
                    }
                    
                    if (binaryCode == memberCode) binaryCode = null;
                    if (!string.IsNullOrEmpty(binaryCode) && !isUnderDownline)
                    {
                        return new MemberBinaryTeamResponseDto
                        {
                            Membercode = memberCode,
                            BinaryTree = new List<MemberBinaryNodeDto>()
                        };
                    }

                    // Query ข้อมูลหลัก
                    string query = "SELECT Membercode, PositionLevel, ChildCode, Membername, Sponsername, Memberposition, MemberpositionName, MemberpositionRanking";
                    query += ", MemberpositionRankingName, PersonalPV, LeftCountActive, RightCountActive, LeftBal, Rightbal, TotalBalance, CurrentLeftPV, CurrentRightPV";
                    query += ", BWDLeftPV, BWDRightPV, NewLeft, NewRight, Maxto2, TName1, TName2, EName1, EName2, Travelpoint1, travelpoint2";
                    query += ", CurrentMonthQualifyPV, LastMonthQualifyPV, LastMonthQualifyStatus, CurrentMonthQualifyStatus, FirstQdate";
                    query += ", CurrentMonth, NextCMonth, CurrentMonth1, LastCMonth, MemberPositionPicture";
                    query += ", TotalLeftBalance, TotalRightBalance, NextPosition, NextPosaddLeftBalance, NextPosaddRightBalance";
                    query += " FROM [000_Member_Binary_Team] (nolock) ";
                    query += " WHERE Membercode = @Membercode";

                    result = new MemberBinaryTeamResponseDto();
                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", binaryCode ?? memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            result.BinaryTree = new List<MemberBinaryNodeDto>();
                            while (await reader.ReadAsync())
                            {
                                var item = MapToResponseDto(reader);
                                result.BinaryTree.Add(item);
                            }
                        }
                    }
                    // ParentCode
                    if (result.BinaryTree.Count > 0 && binaryCode != null)
                    {
                        string parentCode = "";
                        string checkDownlineQuery = @"
                            WITH PureTree AS (
                                SELECT MemberCode AS UplineCode, Member_1_1_L AS DownlineCode 
                                FROM [dbo].[MemberLevel] 
                                WHERE Member_1_1_L IS NOT NULL AND Member_1_1_L <> ''
        
                                UNION ALL
        
                                SELECT MemberCode AS UplineCode, Member_1_2_R AS DownlineCode 
                                FROM [dbo].[MemberLevel] 
                                WHERE Member_1_2_R IS NOT NULL AND Member_1_2_R <> ''
                            ),
                            UplineCTE AS (
                                SELECT UplineCode
                                FROM PureTree
                                WHERE DownlineCode = @DownlineCode
        
                                UNION ALL
        
                                SELECT t.UplineCode
                                FROM PureTree t
                                INNER JOIN UplineCTE c ON t.DownlineCode = c.UplineCode
                                WHERE c.UplineCode <> @UplineCode 
                            )
                            SELECT TOP 1 * FROM UplineCTE 
                            OPTION (MAXRECURSION 0);";

                        using (var cmd = new SqlCommand(checkDownlineQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@UplineCode", memberCode);
                            cmd.Parameters.AddWithValue("@DownlineCode", binaryCode ?? memberCode);

                            var _result = await cmd.ExecuteScalarAsync();

                            if (_result != null && _result != DBNull.Value)
                            {
                                parentCode = Convert.ToString(_result ?? "");
                            }
                        }
                        result.BinaryTree[0].ParentCode = parentCode;
                    }
                }
            }
            catch (Exception ex)
            {
                return new MemberBinaryTeamResponseDto { Membercode = "", Error = ex.Message };
            }

            return result ?? new MemberBinaryTeamResponseDto { Membercode = "" };
        }

        // ==========================================
        // ฟังก์ชันหา ซ้ายสุด / ขวาสุด และคืนค่าเป็น DTO โครงสร้าง Tree
        // ==========================================
        public async Task<MemberBinaryTeamResponseDto> GetExtremeBinaryPathAsync(string rootCode, string direction)
        {
            // ถ้ารหัสต้นทางว่าง ให้พยายามดึงจาก JWT Token ปัจจุบัน
            if (string.IsNullOrEmpty(rootCode))
            {
                rootCode = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(rootCode))
                {
                    return new MemberBinaryTeamResponseDto { Membercode = "" };
                }
            }

            string targetField = direction.ToLower() == "left" ? "Member_1_1_L" : "Member_1_2_R";
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string extremeNodeCode = string.Empty;

            // Query หาสุดสายงาน
            string query = $@"
                WITH ExtremeCTE AS (
                    SELECT 
                        MemberCode AS CurrentMember, 
                        {targetField} AS NextCode, 
                        1 AS DepthLevel
                    FROM [dbo].[MemberLevel]
                    WHERE MemberCode = @RootCode

                    UNION ALL

                    SELECT 
                        t.MemberCode AS CurrentMember, 
                        t.{targetField} AS NextCode, 
                        c.DepthLevel + 1
                    FROM [dbo].[MemberLevel] t
                    INNER JOIN ExtremeCTE c ON t.MemberCode = c.NextCode
                    WHERE c.NextCode IS NOT NULL AND c.NextCode <> ''
                )
                SELECT TOP 1 CurrentMember 
                FROM ExtremeCTE
                ORDER BY DepthLevel DESC
                OPTION (MAXRECURSION 0);";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@RootCode", rootCode);
                        var result = await cmd.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            extremeNodeCode = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new MemberBinaryTeamResponseDto { Membercode = "", Error = ex.Message };
            }

            // ถ้าหาไม่เจอ หรือเกิดข้อผิดพลาด
            if (string.IsNullOrEmpty(extremeNodeCode))
            {
                return new MemberBinaryTeamResponseDto { Membercode = "" };
            }

            // นำรหัสซ้ายสุด/ขวาสุดที่หาได้ ไปดึง Tree Data โดยใช้ GetDisplayAsync ตัวเดิม
            return await GetDisplayAsync(extremeNodeCode, direction);
        }

        #region DataReader Mapping Methods

        private MemberBinaryNodeDto MapToResponseDto(SqlDataReader reader)
        {
            return new MemberBinaryNodeDto
            {
                Membercode = GetString(reader, "Membercode"),
                PositionLevel = GetString(reader, "PositionLevel"),
                ChildCode = GetString(reader, "ChildCode"),
                Membername = GetString(reader, "Membername"),
                Sponsername = GetString(reader, "Sponsername"),
                Memberposition = GetString(reader, "Memberposition"),
                MemberpositionName = GetString(reader, "MemberpositionName"),
                MemberpositionRanking = GetString(reader, "MemberpositionRanking"),
                MemberpositionRankingName = GetString(reader, "MemberpositionRankingName"),
                PersonalPV = GetDecimal(reader, "PersonalPV"),
                LeftCountActive = GetInt(reader, "LeftCountActive"),
                RightCountActive = GetInt(reader, "RightCountActive"),
                LeftBal = GetDecimal(reader, "LeftBal"),
                Rightbal = GetDecimal(reader, "Rightbal"),
                TotalBalance = GetDecimal(reader, "TotalBalance"),
                CurrentLeftPV = GetDecimal(reader, "CurrentLeftPV"),
                CurrentRightPV = GetDecimal(reader, "CurrentRightPV"),
                BWDLeftPV = GetDecimal(reader, "BWDLeftPV"),
                BWDRightPV = GetDecimal(reader, "BWDRightPV"),
                NewLeft = GetDecimal(reader, "NewLeft"),
                NewRight = GetDecimal(reader, "NewRight"),
                Maxto2 = GetDecimal(reader, "Maxto2"),
                TName1 = GetString(reader, "TName1"),
                TName2 = GetString(reader, "TName2"),
                EName1 = GetString(reader, "EName1"),
                EName2 = GetString(reader, "EName2"),
                Travelpoint1 = GetDecimal(reader, "Travelpoint1"),
                Travelpoint2 = GetDecimal(reader, "Travelpoint2"),
                CurrentMonthQualifyPV = GetDecimal(reader, "CurrentMonthQualifyPV"),
                LastMonthQualifyPV = GetDecimal(reader, "LastMonthQualifyPV"),
                LastMonthQualifyStatus = GetString(reader, "LastMonthQualifyStatus"),
                CurrentMonthQualifyStatus = GetString(reader, "CurrentMonthQualifyStatus"),
                FirstQdate = GetString(reader, "FirstQdate"),
                CurrentMonth = GetString(reader, "CurrentMonth"),
                NextCMonth = GetString(reader, "NextCMonth"),
                CurrentMonth1 = GetString(reader, "CurrentMonth1"),
                LastCMonth = GetString(reader, "LastCMonth"),
                MemberPositionPicture = GetString(reader, "MemberPositionPicture"),
                TotalLeftBalance = GetDecimal(reader, "TotalLeftBalance"),
                TotalRightBalance = GetDecimal(reader, "TotalRightBalance"),
                NextPosition = GetString(reader, "NextPosition"),
                NextPosaddLeftBalance = GetDecimal(reader, "NextPosaddLeftBalance"),
                NextPosaddRightBalance = GetDecimal(reader, "NextPosaddRightBalance")
            };
        }

        private string GetString(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private decimal? GetDecimal(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return Convert.ToDecimal(reader.GetValue(ordinal));
            }
            catch
            {
                return null;
            }
        }

        private int? GetInt(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return Convert.ToInt32(reader.GetValue(ordinal));
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

    // DTO Classes
    public class MemberBinaryNodeDto
    {
        public string Membercode { get; set; }
        public string PositionLevel { get; set; }
        public string ChildCode { get; set; }
        public string Membername { get; set; }
        public string Sponsername { get; set; }
        public string Memberposition { get; set; }
        public string MemberpositionName { get; set; }
        public string MemberpositionRanking { get; set; }
        public string MemberpositionRankingName { get; set; }
        public decimal? PersonalPV { get; set; }
        public int? LeftCountActive { get; set; }
        public int? RightCountActive { get; set; }
        public decimal? LeftBal { get; set; }
        public decimal? Rightbal { get; set; }
        public decimal? TotalBalance { get; set; }
        public decimal? CurrentLeftPV { get; set; }
        public decimal? CurrentRightPV { get; set; }
        public decimal? BWDLeftPV { get; set; }
        public decimal? BWDRightPV { get; set; }
        public decimal? NewLeft { get; set; }
        public decimal? NewRight { get; set; }
        public decimal? Maxto2 { get; set; }
        public string TName1 { get; set; }
        public string TName2 { get; set; }
        public string EName1 { get; set; }
        public string EName2 { get; set; }
        public decimal? Travelpoint1 { get; set; }
        public decimal? Travelpoint2 { get; set; }
        public decimal? CurrentMonthQualifyPV { get; set; }
        public decimal? LastMonthQualifyPV { get; set; }
        public string LastMonthQualifyStatus { get; set; }
        public string CurrentMonthQualifyStatus { get; set; }
        public string FirstQdate { get; set; }
        public string CurrentMonth { get; set; }
        public string NextCMonth { get; set; }
        public string CurrentMonth1 { get; set; }
        public string LastCMonth { get; set; }
        public string MemberPositionPicture { get; set; }
        public decimal? TotalLeftBalance { get; set; }
        public decimal? TotalRightBalance { get; set; }
        public string NextPosition { get; set; }
        public decimal? NextPosaddLeftBalance { get; set; }
        public decimal? NextPosaddRightBalance { get; set; }

        public string ParentCode { get; set; }
        public bool IsEmptyNode { get; set; }
        public List<MemberBinaryNodeDto> Children { get; set; }
    }

    public class MemberBinaryTeamResponseDto
    {
        public string Membercode { get; set; }
        public string PositionLevel { get; set; }
        public string ChildCode { get; set; }
        public string Membername { get; set; }
        public string Sponsername { get; set; }
        public string Memberposition { get; set; }
        public string MemberpositionName { get; set; }
        public string MemberpositionRanking { get; set; }
        public string MemberpositionRankingName { get; set; }
        public decimal? PersonalPV { get; set; }
        public int? LeftCountActive { get; set; }
        public int? RightCountActive { get; set; }
        public decimal? LeftBal { get; set; }
        public decimal? Rightbal { get; set; }
        public decimal? TotalBalance { get; set; }
        public decimal? CurrentLeftPV { get; set; }
        public decimal? CurrentRightPV { get; set; }
        public decimal? BWDLeftPV { get; set; }
        public decimal? BWDRightPV { get; set; }
        public decimal? NewLeft { get; set; }
        public decimal? NewRight { get; set; }
        public decimal? Maxto2 { get; set; }
        public string TName1 { get; set; }
        public string TName2 { get; set; }
        public string EName1 { get; set; }
        public string EName2 { get; set; }
        public decimal? Travelpoint1 { get; set; }
        public decimal? Travelpoint2 { get; set; }
        public decimal? CurrentMonthQualifyPV { get; set; }
        public decimal? LastMonthQualifyPV { get; set; }
        public string LastMonthQualifyStatus { get; set; }
        public string CurrentMonthQualifyStatus { get; set; }
        public string FirstQdate { get; set; }
        public string CurrentMonth { get; set; }
        public string NextCMonth { get; set; }
        public string CurrentMonth1 { get; set; }
        public string LastCMonth { get; set; }
        public string MemberPositionPicture { get; set; }
        public decimal? TotalLeftBalance { get; set; }
        public decimal? TotalRightBalance { get; set; }
        public string NextPosition { get; set; }
        public decimal? NextPosaddLeftBalance { get; set; }
        public decimal? NextPosaddRightBalance { get; set; }

        public List<MemberBinaryNodeDto> BinaryTree { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Error { get; set; }
    }
}