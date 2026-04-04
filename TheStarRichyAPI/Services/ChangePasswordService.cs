using System.Data.SqlClient;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IChangePasswordService
    {
        Task<(bool success, string message)> CheckOldPasswordAsync(string oldPassword);
        Task<(bool success, string message)> UpdatePasswordAsync(string newPassword);
    }

    public class ChangePasswordService : IChangePasswordService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GetPasskeyAsync(string column)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string value = "";
            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();
                using var cmd = new SqlCommand($"SELECT {column} FROM S02", con);
                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows && await reader.ReadAsync() && !reader.IsDBNull(0))
                    value = reader.GetString(0);
            }
            catch { }
            return value;
        }

        private bool IsValidPasskey(string passkey, string pk1, string pk2)
            => !string.IsNullOrEmpty(passkey) && (passkey == pk1 || passkey == pk2);

        private string GetMemberCode()
            => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        public async Task<(bool success, string message)> CheckOldPasswordAsync(string oldPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
                return (false, "กรุณากรอกรหัสผ่านเก่า");

            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"] ?? "";
            string pk1 = await GetPasskeyAsync("Passkey1");
            string pk2 = await GetPasskeyAsync("Passkey2");
            if (!IsValidPasskey(passkey, pk1, pk2))
                return (false, "Unauthorized");

            string memberCode = GetMemberCode();
            if (string.IsNullOrEmpty(memberCode))
                return (false, "ไม่พบข้อมูลสมาชิก");

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();
                // Use the ENCODE function to hash the typed password, then match against stored hash
                string sql = @"SELECT COUNT(1) FROM [000_Member_info_for_mobile] (NOLOCK)
                               WHERE Membercode = @Membercode
                                 AND Password = dbo.ENCODE(@Password)";
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Membercode", memberCode);
                cmd.Parameters.AddWithValue("@Password", oldPassword);
                int count = (int)await cmd.ExecuteScalarAsync();
                if (count > 0)
                    return (true, "รหัสผ่านเก่าถูกต้อง");
                return (false, "รหัสผ่านเก่าไม่ถูกต้อง");
            }
            catch (Exception ex)
            {
                return (false, $"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdatePasswordAsync(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "กรุณากรอกรหัสผ่านใหม่");

            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"] ?? "";
            string pk1 = await GetPasskeyAsync("Passkey1");
            string pk2 = await GetPasskeyAsync("Passkey2");
            if (!IsValidPasskey(passkey, pk1, pk2))
                return (false, "Unauthorized");

            string memberCode = GetMemberCode();
            if (string.IsNullOrEmpty(memberCode))
                return (false, "ไม่พบข้อมูลสมาชิก");

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();
                string sql = @"UPDATE dbo.M06
                               SET M06_X62 = dbo.ENCODE(@NewPassword)
                               WHERE M06_PX1 = @Membercode";
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);
                cmd.Parameters.AddWithValue("@Membercode", memberCode);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows > 0)
                    return (true, "เปลี่ยนรหัสผ่านสำเร็จ");
                return (false, "ไม่พบข้อมูลสมาชิกที่ต้องการอัพเดท");
            }
            catch (Exception ex)
            {
                return (false, $"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }
    }
}
