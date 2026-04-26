using System.Data.SqlClient;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IReportMemberWitholdingTaxService
    {
        Task<List<dynamic>> GetHeaderAsync();
    }

    public class ReportMemberWitholdingTaxService : IReportMemberWitholdingTaxService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportMemberWitholdingTaxService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GetPasskeyAsync(string column)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string password = "";
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = $"SELECT {column} FROM S02";
                    using (var command = new SqlCommand(query, con))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows && await reader.ReadAsync() && !reader.IsDBNull(0))
                            password = reader.GetString(0);
                    }
                }
            }
            catch { }
            return password;
        }

        private bool IsValidPasskey(string passkey, string pk1, string pk2)
            => !string.IsNullOrEmpty(passkey) && (passkey == pk1 || passkey == pk2);

        public async Task<List<dynamic>> GetHeaderAsync()
        {
            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"] ?? "";
            string pk1 = await GetPasskeyAsync("Passkey1");
            string pk2 = await GetPasskeyAsync("Passkey2");
            if (!IsValidPasskey(passkey, pk1, pk2)) return new List<dynamic>();

            string memberCode = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (string.IsNullOrEmpty(memberCode)) return new List<dynamic>();

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    const string query = @"SELECT *
                                           FROM [000_Member_witholdingtax] (nolock)
                                           WHERE Membercode = @Membercode";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic row = new System.Dynamic.ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;
                                for (int i = 0; i < reader.FieldCount; i++)
                                    rowDict[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch { }

            return result;
        }
    }
}
