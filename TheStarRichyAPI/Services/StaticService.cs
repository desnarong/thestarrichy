using System.Data.SqlClient;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IStaticService
    {
        Task<List<dynamic>> GetBankAsync();
        Task<List<dynamic>> GetCountryAsync();
        Task<List<dynamic>> GetCountryBusinessAsync();
        Task<List<dynamic>> GetDistrictAsync();
        Task<List<dynamic>> GetTitlenameAsync();
    }
    public class StaticService : IStaticService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StaticService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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
        public async Task<List<dynamic>> GetBankAsync()
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_Bank]";

                    using (var command = new SqlCommand(query, con))
                    {
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

            return result.Count > 0 ? result : new List<dynamic>();
        }
        public async Task<List<dynamic>> GetCountryAsync()
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_Country]";

                    using (var command = new SqlCommand(query, con))
                    {
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

            return result.Count > 0 ? result : new List<dynamic>();
        }
        public async Task<List<dynamic>> GetCountryBusinessAsync()
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_Country_business]";

                    using (var command = new SqlCommand(query, con))
                    {
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

            return result.Count > 0 ? result : new List<dynamic>();
        }
        public async Task<List<dynamic>> GetDistrictAsync()
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_District]";

                    using (var command = new SqlCommand(query, con))
                    {
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

            return result.Count > 0 ? result : new List<dynamic>();
        }
        public async Task<List<dynamic>> GetTitlenameAsync()
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_Titlename]";

                    using (var command = new SqlCommand(query, con))
                    {
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

            return result.Count > 0 ? result : new List<dynamic>();
        }
    }
}
