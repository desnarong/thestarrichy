using System.Data.SqlClient;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IReportMemberOrderInvoiceDetailService
    {
        Task<dynamic> GetInvoiceDetailAsync(string billNo);
    }

    public class ReportMemberOrderInvoiceDetailService : IReportMemberOrderInvoiceDetailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportMemberOrderInvoiceDetailService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<dynamic> GetInvoiceDetailAsync(string billNo)
        {
            if (string.IsNullOrWhiteSpace(billNo)) return null;

            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"] ?? "";
            string pk1 = await GetPasskeyAsync("Passkey1");
            string pk2 = await GetPasskeyAsync("Passkey2");
            if (!IsValidPasskey(passkey, pk1, pk2)) return null;

            string memberCode = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (string.IsNullOrEmpty(memberCode)) return null;

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    // Get header data from 000_Member_Order_for_mobile
                    string headerQuery = @"SELECT * FROM [000_Member_Order_Invoice] (nolock)
                                           WHERE BillNo = @BillNo";// AND Membercode = @Membercode

                    var headerData = new Dictionary<string, object>();
                    
                    using (var headerCommand = new SqlCommand(headerQuery, con))
                    {
                        headerCommand.Parameters.AddWithValue("@BillNo", billNo);
                        //headerCommand.Parameters.AddWithValue("@Membercode", memberCode);

                        using (var headerReader = await headerCommand.ExecuteReaderAsync())
                        {
                            if (await headerReader.ReadAsync())
                            {
                                for (int i = 0; i < headerReader.FieldCount; i++)
                                {
                                    string columnName = headerReader.GetName(i);
                                    object columnValue = headerReader.IsDBNull(i) ? null : headerReader.GetValue(i);
                                    headerData[columnName] = columnValue;
                                }
                            }
                        }
                    }

                    // Get detail data from 000_Member_Order_Invoice_detail
                    string detailQuery = @"SELECT * FROM [000_Member_Order_Invoice_detail] (nolock)
                                           WHERE BillNo = @BillNo
                                           ORDER BY No";

                    var detailList = new List<dynamic>();

                    using (var detailCommand = new SqlCommand(detailQuery, con))
                    {
                        detailCommand.Parameters.AddWithValue("@BillNo", billNo);

                        using (var detailReader = await detailCommand.ExecuteReaderAsync())
                        {
                            while (await detailReader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < detailReader.FieldCount; i++)
                                {
                                    string columnName = detailReader.GetName(i);
                                    object columnValue = detailReader.IsDBNull(i) ? null : detailReader.GetValue(i);
                                    row[columnName] = columnValue;
                                }

                                detailList.Add(row);
                            }
                        }
                    }

                    if (headerData.Count == 0 || detailList.Count == 0) return null;

                    return new
                    {
                        success = true,
                        header = headerData,
                        details = detailList
                    };
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
