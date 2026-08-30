using System.Data.SqlClient;
using System.Dynamic;

namespace TheStarRichyApi.Services
{
    public interface IDocumentDownloadService
    {
        /// <summary>
        /// ดึงรายการเอกสารทั้งหมดจาก view [000_download]
        /// </summary>
        Task<List<dynamic>> GetAllDocumentsAsync();
    }

    public class DocumentDownloadService : IDocumentDownloadService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentDownloadService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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

        private bool ValidatePasskey(string passkey)
        {
            if (string.IsNullOrEmpty(passkey))
                return false;

            string passwordEncode1 = GetPasskeyAsync("Passkey1").GetAwaiter().GetResult();
            string passwordEncode2 = GetPasskeyAsync("Passkey2").GetAwaiter().GetResult();

            return passkey == passwordEncode1 || passkey == passwordEncode2;
        }

        /// <summary>
        /// ดึงรายการเอกสารทั้งหมดจาก view [000_download]
        /// ประกอบด้วย: num (ลำดับ), filedescription (รายละเอียด), filelocationname (ชื่อไฟล์)
        /// </summary>
        public async Task<List<dynamic>> GetAllDocumentsAsync()
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey) || !ValidatePasskey(passkey))
            {
                return new List<dynamic>();
            }

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = @"SELECT num, filedescription, filelocationname 
                                     FROM [000_download] 
                                     ORDER BY num";

                    using (var command = new SqlCommand(query, con))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic row = new ExpandoObject();
                                var rowDict = (IDictionary<string, object>)row;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    rowDict[columnName] = columnValue;
                                }

                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new List<dynamic>();
            }

            return result.Count > 0 ? result : new List<dynamic>();
        }
    }
}
