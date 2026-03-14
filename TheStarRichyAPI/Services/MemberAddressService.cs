using System.Data.SqlClient;
using System.Security.Claims;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    public interface IMemberAddressService
    {
        Task<List<MemberAddressDto>> GetAddressesAsync();
        Task<(bool Success, string Message)> UpsertAddressesAsync(List<MemberAddressDto> addresses);
    }

    public class MemberAddressService : IMemberAddressService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberAddressService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // ============================================================
        // Passkey / Auth helpers (shared pattern with other services)
        // ============================================================
        private async Task<string> GetPasskeyAsync(string column)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string password = "";
            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();
                string query = $"SELECT {column} FROM S02";
                using var cmd = new SqlCommand(query, con);
                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows && await reader.ReadAsync() && !reader.IsDBNull(0))
                    password = reader.GetString(0);
            }
            catch { }
            return password;
        }

        private async Task<bool> ValidateRequestAsync()
        {
            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"] ?? string.Empty;
            if (string.IsNullOrEmpty(passkey)) return false;

            string key1 = await GetPasskeyAsync("Passkey1");
            string key2 = await GetPasskeyAsync("Passkey2");
            return passkey == key1 || passkey == key2;
        }

        private string? GetMemberCode()
            => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // ============================================================
        // GET — ดึงที่อยู่ทุกประเภทของสมาชิก
        // ============================================================
        public async Task<List<MemberAddressDto>> GetAddressesAsync()
        {
            if (!await ValidateRequestAsync()) return new List<MemberAddressDto>();

            string? memberCode = GetMemberCode();
            if (string.IsNullOrEmpty(memberCode)) return new List<MemberAddressDto>();

            var result = new List<MemberAddressDto>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();

                const string sql = @"
                    SELECT
                        a.Id,
                        a.AddressType,
                        a.HouseNumber,
                        a.Moo,
                        a.Alley,
                        a.Road,
                        a.Building,
                        a.Floor,
                        a.Other,
                        a.TambonId,
                        a.Zipcode,
                        a.CompanyName,
                        a.CompanyTaxId,
                        a.BranchCode,
                        a.BranchName,
                        t.tambon       AS Tambon,
                        t.amphoe       AS Amphoe,
                        t.province     AS Province,
                        t.tambon_code  AS TambonCode,
                        t.amphoe_code  AS AmphoeCode,
                        t.province_code AS ProvinceCode,
                        ISNULL(a.Zipcode, t.zipcode) AS EffectiveZipcode
                    FROM M06_Addresses a
                    LEFT JOIN TBL_TAMBONS t ON t.id = a.TambonId
                    WHERE a.MemberCode = @MemberCode
                      AND a.IsActive = 1
                    ORDER BY a.AddressType";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@MemberCode", memberCode);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    // Effective zipcode: prefer stored Zipcode, fallback to tambon.zipcode
                    string effectiveZip = reader["EffectiveZipcode"] == DBNull.Value
                        ? string.Empty
                        : reader["EffectiveZipcode"].ToString()!;

                    result.Add(new MemberAddressDto
                    {
                        Id          = reader["Id"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Id"]),
                        AddressType = Convert.ToInt32(reader["AddressType"]),
                        HouseNumber = GetString(reader, "HouseNumber"),
                        Moo         = GetString(reader, "Moo"),
                        Alley       = GetString(reader, "Alley"),
                        Road        = GetString(reader, "Road"),
                        Building    = GetString(reader, "Building"),
                        Floor       = GetString(reader, "Floor"),
                        Other       = GetString(reader, "Other"),
                        TambonId    = reader["TambonId"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["TambonId"]),
                        Zipcode     = effectiveZip,
                        CompanyName  = GetString(reader, "CompanyName"),
                        CompanyTaxId = GetString(reader, "CompanyTaxId"),
                        BranchCode   = GetString(reader, "BranchCode"),
                        BranchName   = GetString(reader, "BranchName"),
                        Tambon       = GetString(reader, "Tambon"),
                        Amphoe       = GetString(reader, "Amphoe"),
                        Province     = GetString(reader, "Province"),
                        TambonCode   = GetString(reader, "TambonCode"),
                        AmphoeCode   = GetString(reader, "AmphoeCode"),
                        ProvinceCode = GetString(reader, "ProvinceCode"),
                    });
                }
            }
            catch (Exception ex)
            {
                // Log in production
                _ = ex;
            }

            return result;
        }

        // ============================================================
        // PUT — Upsert (MERGE) ที่อยู่
        // ============================================================
        public async Task<(bool Success, string Message)> UpsertAddressesAsync(List<MemberAddressDto> addresses)
        {
            if (!await ValidateRequestAsync())
                return (false, "Unauthorized");

            string? memberCode = GetMemberCode();
            if (string.IsNullOrEmpty(memberCode))
                return (false, "ไม่พบข้อมูลสมาชิก");

            if (addresses == null || addresses.Count == 0)
                return (false, "ไม่มีข้อมูลที่อยู่ที่ส่งมา");

            // Validate type values
            var validTypes = new HashSet<int> { 1, 2, 3 };
            foreach (var addr in addresses)
            {
                if (!validTypes.Contains(addr.AddressType))
                    return (false, $"AddressType ไม่ถูกต้อง: {addr.AddressType}");
            }

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();

                foreach (var addr in addresses)
                {
                    const string sql = @"
                        MERGE INTO M06_Addresses AS target
                        USING (SELECT @MemberCode AS MemberCode, @AddressType AS AddressType) AS src
                        ON target.MemberCode = src.MemberCode AND target.AddressType = src.AddressType
                        WHEN MATCHED THEN
                            UPDATE SET
                                HouseNumber  = @HouseNumber,
                                Moo          = @Moo,
                                Alley        = @Alley,
                                Road         = @Road,
                                Building     = @Building,
                                Floor        = @Floor,
                                [Other]      = @Other,
                                TambonId     = @TambonId,
                                Zipcode      = @Zipcode,
                                CompanyName  = @CompanyName,
                                CompanyTaxId = @CompanyTaxId,
                                BranchCode   = @BranchCode,
                                BranchName   = @BranchName,
                                UpdatedAt    = GETDATE(),
                                UpdatedBy    = @MemberCode,
                                IsActive     = 1
                        WHEN NOT MATCHED THEN
                            INSERT (MemberCode, AddressType, HouseNumber, Moo, Alley, Road,
                                    Building, Floor, [Other], TambonId, Zipcode,
                                    CompanyName, CompanyTaxId, BranchCode, BranchName,
                                    CreatedAt, UpdatedBy, IsActive)
                            VALUES (@MemberCode, @AddressType, @HouseNumber, @Moo, @Alley, @Road,
                                    @Building, @Floor, @Other, @TambonId, @Zipcode,
                                    @CompanyName, @CompanyTaxId, @BranchCode, @BranchName,
                                    GETDATE(), @MemberCode, 1);";

                    using var cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@MemberCode",  memberCode);
                    cmd.Parameters.AddWithValue("@AddressType", addr.AddressType);
                    cmd.Parameters.AddWithValue("@HouseNumber", (object?)NullIfEmpty(addr.HouseNumber) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Moo",         (object?)NullIfEmpty(addr.Moo)         ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Alley",       (object?)NullIfEmpty(addr.Alley)       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Road",        (object?)NullIfEmpty(addr.Road)        ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Building",    (object?)NullIfEmpty(addr.Building)    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Floor",       (object?)NullIfEmpty(addr.Floor)       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Other",       (object?)NullIfEmpty(addr.Other)       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TambonId",    (object?)(addr.TambonId.HasValue && addr.TambonId > 0 ? addr.TambonId : null) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Zipcode",     (object?)NullIfEmpty(addr.Zipcode)     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyName", (object?)NullIfEmpty(addr.CompanyName) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyTaxId",(object?)NullIfEmpty(addr.CompanyTaxId)?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchCode",  (object?)NullIfEmpty(addr.BranchCode)  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchName",  (object?)NullIfEmpty(addr.BranchName)  ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }

                return (true, "บันทึกที่อยู่เรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                return (false, $"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }

        // ============================================================
        // Helpers
        // ============================================================
        private static string? GetString(SqlDataReader reader, string column)
            => reader[column] == DBNull.Value ? null : reader[column]?.ToString()?.Trim();

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
