using System.Data;
using System.Data.SqlClient;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    public interface ISaleOrderSyncService
    {
        Task<SyncOrderToT05Response> SyncToT05Async(SyncOrderToT05Request request);
        Task<bool> ValidatePasskeyAsync();
    }

    public class SaleOrderSyncService : ISaleOrderSyncService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SaleOrderSyncService> _logger;

        public SaleOrderSyncService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SaleOrderSyncService> logger)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private async Task<string> GetPasskeyAsync(string column)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting passkey");
            }

            return password;
        }

        public async Task<bool> ValidatePasskeyAsync()
        {
            string passkey = _httpContextAccessor.HttpContext?.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
                return false;

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            return passkey == passwordEncode1 || passkey == passwordEncode2;
        }

        /// <summary>
        /// เรียก SP_SyncOrderToT05 เพื่อ sync order เข้า T05
        /// </summary>
        public async Task<SyncOrderToT05Response> SyncToT05Async(SyncOrderToT05Request request)
        {
            var response = new SyncOrderToT05Response();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    using (var command = new SqlCommand("SP_SyncOrderToT05", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Input parameters
                        command.Parameters.AddWithValue("@SaleNo", request.SaleNo);
                        command.Parameters.AddWithValue("@Membercode", request.Membercode);
                        command.Parameters.AddWithValue("@Billtype", request.Billtype);
                        command.Parameters.AddWithValue("@SaleDate", request.SaleDate);
                        command.Parameters.AddWithValue("@CreateBy", request.CreateBy);
                        command.Parameters.AddWithValue("@PV", request.PV);
                        command.Parameters.AddWithValue("@Amount", request.Amount);

                        // TVP: dbo.SalesDetailType
                        var detailTable = CreateSalesDetailTable(request.Detail);
                        var tvpParam = command.Parameters.AddWithValue("@Detail", detailTable);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "dbo.SalesDetailType";

                        // Output parameter: @OrderID
                        var orderIdParam = new SqlParameter("@OrderID", SqlDbType.NVarChar, 20)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(orderIdParam);

                        await command.ExecuteNonQueryAsync();

                        // Read output
                        if (orderIdParam.Value != DBNull.Value)
                        {
                            response.OrderID = orderIdParam.Value.ToString();
                        }

                        response.Success = !string.IsNullOrEmpty(response.OrderID);
                        response.Message = response.Success
                            ? $"Sync order สำเร็จ OrderID: {response.OrderID}"
                            : "ไม่สามารถสร้าง Order ได้";

                        _logger.LogInformation(
                            "SP_SyncOrderToT05 completed. SaleNo: {SaleNo}, Membercode: {Membercode}, OrderID: {OrderID}",
                            request.SaleNo, request.Membercode, response.OrderID);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calling SP_SyncOrderToT05 for SaleNo: {SaleNo}, Membercode: {Membercode}",
                    request.SaleNo, request.Membercode);

                response.Success = false;
                response.Message = $"เกิดข้อผิดพลาด: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// สร้าง DataTable สำหรับ TVP dbo.SalesDetailType (15 columns)
        /// </summary>
        private DataTable CreateSalesDetailTable(List<SyncOrderDetail> details)
        {
            var dt = new DataTable();

            // 15 คอลัมน์ต้องเรียงและตั้งชื่อตรงกับ dbo.SalesDetailType
            dt.Columns.Add("SeqNo", typeof(int));
            dt.Columns.Add("SaleID", typeof(string));
            dt.Columns.Add("CustomerCode", typeof(string));
            dt.Columns.Add("SaleDate", typeof(DateTime));
            dt.Columns.Add("Billtype", typeof(string));
            dt.Columns.Add("ItemCode", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("Price_unit", typeof(decimal));
            dt.Columns.Add("PV_unit", typeof(decimal));
            dt.Columns.Add("TotalAmount", typeof(decimal));
            dt.Columns.Add("TotalPV", typeof(decimal));
            dt.Columns.Add("CreateBy", typeof(string));
            dt.Columns.Add("CreateDate", typeof(DateTime));
            dt.Columns.Add("ReferorderID", typeof(string));
            dt.Columns.Add("Mainset", typeof(string));

            foreach (var item in details)
            {
                dt.Rows.Add(
                    item.SeqNo,
                    item.SaleID ?? string.Empty,
                    item.CustomerCode ?? string.Empty,
                    item.SaleDate,
                    item.Billtype ?? string.Empty,
                    item.ItemCode ?? string.Empty,
                    item.Qty,
                    item.Price_unit,
                    item.PV_unit,
                    item.TotalAmount,
                    item.TotalPV,
                    item.CreateBy ?? string.Empty,
                    item.CreateDate,
                    item.ReferorderID ?? string.Empty,
                    item.Mainset ?? string.Empty
                );
            }

            return dt;
        }
    }
}
