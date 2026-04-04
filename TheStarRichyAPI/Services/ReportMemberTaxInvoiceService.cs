using System.Data.SqlClient;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IReportMemberTaxInvoiceService
    {
        Task<List<dynamic>> GetHeaderAsync(string? fromDate, string? toDate);
        Task<List<dynamic>> GetDetailAsync(string? billNo);
    }

    public class ReportMemberTaxInvoiceService : IReportMemberTaxInvoiceService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportMemberTaxInvoiceService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<dynamic>> GetHeaderAsync(string? fromDate, string? toDate)
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

                    string query = @"SELECT Create_date, BillNo, BillDate, ReferenceNo,
                        Companyname, Companyaddress, Companytelephone, CompanyTaxID,
                        Membercode, CustomerName, CustomerTaxID, CustomerAddress, CustomerTelephone,
                        Createby, BilltypeCode, PaymentType, DeliveryAddress,
                        TotalPV, TotalAmount, ShippingFee, S02_X11,
                        Pricebeforetax, Taxfee, Pricebeincludetax
                        FROM [000_Member_Order_Tax_Invoice] (nolock)
                        WHERE Membercode = @Membercode";

                    var conditions = new List<string>();
                    if (!string.IsNullOrEmpty(fromDate))
                        conditions.Add("BillDate >= @FromDate");
                    if (!string.IsNullOrEmpty(toDate))
                        conditions.Add("BillDate <= @ToDate");
                    if (conditions.Count > 0)
                        query += " AND " + string.Join(" AND ", conditions);

                    query += " ORDER BY BillDate DESC";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);
                        if (!string.IsNullOrEmpty(fromDate))
                            command.Parameters.AddWithValue("@FromDate", fromDate);
                        if (!string.IsNullOrEmpty(toDate))
                            command.Parameters.AddWithValue("@ToDate", toDate + " 23:59:59");

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

        public async Task<List<dynamic>> GetDetailAsync(string? billNo)
        {
            // billNo is required — never return all rows
            if (string.IsNullOrWhiteSpace(billNo)) return new List<dynamic>();

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

                    // Use EXISTS instead of JOIN to avoid row duplication when header has multiple rows per BillNo
                    string query = @"SELECT d.No, d.BillNo, d.ProductCode, d.ProductName,
                        d.Quantity, d.Unitname, d.PriceperUnit, d.TotalAmount, d.TotalPV, d.PV
                        FROM [000_Member_order_tax_invoice_detail] d (nolock)
                        WHERE d.BillNo = @BillNo
                          AND EXISTS (
                              SELECT 1 FROM [000_Member_Order_Tax_Invoice] h (nolock)
                              WHERE h.BillNo = d.BillNo AND h.Membercode = @Membercode
                          )";

                    query += " ORDER BY d.No";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);
                        command.Parameters.AddWithValue("@BillNo", billNo);

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
