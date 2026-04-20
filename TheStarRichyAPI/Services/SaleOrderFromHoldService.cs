using System.Data.SqlClient;

namespace TheStarRichyApi.Services
{
    public interface ISaleOrderFromHoldService
    {
        Task<List<dynamic>> GetDisplayAsync(string membercode);
    }

    public class SaleOrderFromHoldService : ISaleOrderFromHoldService
    {
        private readonly IConfiguration _configuration;

        public SaleOrderFromHoldService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<dynamic>> GetDisplayAsync(string membercode)
        {
            if (string.IsNullOrWhiteSpace(membercode))
                return new List<dynamic>();

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    string query = @"SELECT [BillNo],[Membercode],[Membername],[BillDate],[ExpireDate]
                                          ,[ProductID],[ProductName],[Unit],[UnitPrice],[PV]
                                          ,[TotalPrice],[Saleout],[Remain],[Billtype],[UnitPV],[ProductSet]
                                     FROM [000_Member_Order_for_mobile]
                                    WHERE Remain > 0
                                      AND billtype = N'โฮล/HOLD'
                                      AND Membercode = @membercode
                                    ORDER BY ProductID, BillNo";

                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@membercode", membercode);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic row = new System.Dynamic.ExpandoObject();
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

            return result;
        }
    }
}
