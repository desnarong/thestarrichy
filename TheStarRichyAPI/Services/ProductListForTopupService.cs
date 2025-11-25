using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace TheStarRichyApi.Services
{
    public interface IProductListForTopupService
    {
        Task<List<dynamic>> GetDisplayAsync(string baseUrl, string groupCode, string proDucttype, string sortOrder, string productid);
    }
    public class ProductListForTopupService : IProductListForTopupService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemberService _memberService;

        public ProductListForTopupService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemberService memberService)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
        }
        public async Task<string> GetPermissionAsync(string column, string memberCode)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string MemberPermission = "";

            try
            {
                using SqlConnection con = new(connectionString);
                await con.OpenAsync();
                string query = $"SELECT {column}  from M06_permission where M06_PX1=@Membercode";

                using SqlCommand command = new(query, con);
                command.Parameters.AddWithValue("@Membercode", memberCode);

                using SqlDataReader reader = await command.ExecuteReaderAsync();
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
            catch (Exception)
            {
                // Log exception in production
            }

            return MemberPermission;
        }

        private static string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return "";

            if (DateTime.TryParse(dateObj.ToString(), out DateTime date))
            {
                return date.ToString("dd/MM/yyyy");
            }
            return "";
        }
        public async Task<string> GetPasskeyAsync(string column)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");
            string password = "";

            try
            {
                using SqlConnection con = new(connectionString);
                await con.OpenAsync();
                string query = $"SELECT {column} FROM S02";
                using SqlCommand command = new(query, con);
                using SqlDataReader reader = await command.ExecuteReaderAsync();
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
            catch (Exception)
            {
                // Log exception in production
            }

            return password;
        }
        public async Task<List<dynamic>> GetDisplayAsync(string baseUrl, string groupCode, string proDucttype, string sortOrder, string productid)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            // Verify Passkey
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                return new List<dynamic> { new { Membercode = "" } };
            }

            // Get Membercode from JWT
            string? memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var memberInfos = await _memberService.GetMember2DisplayAsync(baseUrl);
            var memberInfo = memberInfos.FirstOrDefault(m => m.Membercode == memberCode);
            string? Registerdate = memberInfo?.RegisterDate; //วันสมัคร
            //string? groupCode = _httpContextAccessor.HttpContext.Request.Query["groupcode"];
            //string? proDucttype = _httpContextAccessor.HttpContext.Request.Query["producttype"]; //ประเภทสินค้า 0=ทั้งหมด 1=โปรโมชั่น 2=ขายดี 3=มาใหม่
            //string? sortOrder = _httpContextAccessor.HttpContext.Request.Query["sortorder"]; //การเรียงลำดับ 0=ล่าสุด 1=เก่าสุด 2=ราคาต่ำ-สูง 3=ราคาสูง-ต่ำ 4=PV ต่ำ-สูง 5=PV สูง-ต่ำ
            //string? productid = _httpContextAccessor.HttpContext.Request.Query["productid"]; //ค้นหาจากรหัสสินค้า
            //เงื่อนไขการแสดงสินค้า
            // M01_X37 = '0' or M01_X37 = '2' or M01_X37 = '1'  - สถานะสินค้า 0=ปกติ 1=ระงับ 2=สินค้าพิเศษ
            // M01_X40 = '0' or M01_X40 = '2'  - สถานะสินค้า 0=ปกติ 1=สินค้าหมด 2=สินค้าพิเศษ
            // TypeOfProductCode<>'005' and  M01_X58 = '0' - สินค้าทั่วไป
            // TypeOfProductCode = '005'  or TypeOfProductCode = '006' - สินค้าโปรโมชั่น
            // M01_X58 = '0' - สินค้าทั่วไป  M01_X58 = '1' - สินค้าเฉพาะสมาชิกใหม่
            // M01_X52 - จำกัดการซื้อสินค้าต่อคน
            // M01_X53 - จำกัดการซื้อสินค้าทั้งหมด
            // M01_X59 - วันที่เริ่มซื้อได้
            // M01_X60 - วันที่สิ้นสุดซื้อได้
            // TypeOfProductCode = '005'  or TypeOfProductCode = '006' - สินค้าโปรโมชั่น
            // deleveryType M01_X46='0' - ส่ง   M01_X46='1' -ไม่ส่งรับเอง
            // shippingFee M01_X50='0'- ค่าส่งไม่ฟรี  M01_X50='1'-ค่าส่งฟรี


            if (string.IsNullOrEmpty(memberCode))
            {
                return new List<dynamic> { new { Membercode = "" } };
            }
            if (string.IsNullOrEmpty(Registerdate))
            {
                return new List<dynamic> { new { Registerdate = "" } };
            }

            var result = new List<dynamic>();
            string? connectionString = _configuration.GetConnectionString("MLMConnectionString");
            Registerdate = FormatDate(Registerdate);

            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();


                string query = "SELECT A1.ProductID,[ProductThaiName],[ProductEngName],[ProductDescription]" +
                    ",[ProductDescription1],[ProductGroupcode],[ProductGroupThaiName],[ProductGroupEngName]" +
                    ",[TypeOfProductCode],[TypeOfProduct],[UnitOfProduct_th],[UnitOfProduct_en] " +
                    ",[MemberPrice],[MemberEngPrice],[NormalPrice],[PV],[BV],[Weigth],[Country],[Topupcheck] " +
                    ",[Holdcheck],[Picture1],[Picture2],[Picture3],[Picture4],[Picture5] " +
                    ",[LimitPerMembercode],[LimitPertobuy],[scheduledforsale],[scheduledforposition],[scheduledforpositionrule] " +
                    ",[scheduledfornewmember],[scheduledfornewmemberstardate],[scheduledfornewmemberenddate] " +
                    ",[M01_X58],[M01_X41],[M01_X39],[M01_X37],[M01_X40],[M01_X21],M01_X52,M01_X53,M01_X59,M01_X60,[Totalproduct] " +
                    ",M01_X46,M01_X50,M01_X68,M01_X69" +   //
                    ",d.T03_X5 as Membercode,COALESCE((d.TotalbuyPerson),0) as TotalbuyPerson" +
                    ",COALESCE((e.TotalbuyALL),0)  as TotalbuyALL" +
                    ",S02_X109  as TypeofFee" +  //เงื่อนไขค่าส่งเป็น 0-บาท หรือ 1- พีวี
                    ",S02_X111  as CondFee" +   // ยอดขั้นต่ำที่ส่งฟรี
                    ",S02_X110  as DeleveryFe1" +   // ค่าส่ง 1
                    ",S02_X123  as DeleveryFee2" +   // ค่าส่ง 2
                    " from[000_Product] A1 (nolock) " +

                    " left outer join  " +
                    " (select T03_2X1, T03_X5, COALESCE(Sum(BuyLimit),0) as TotalbuyPerson " +
                    " from Sum_ProductSet where CMonth = FORMAT(GETDATE(), 'yyyy-MM')" +
                    " group by T03_2X1,T03_X5) as d on A1.ProductID = d.T03_2X1  and d.T03_X5 = @Membercode" +
                    " left outer join" +
                    " (select T03_2X1, COALESCE(Sum(BuyLimit), 0) as TotalbuyALL " +
                    " from Sum_ProductSet  group by T03_2X1) as e on A1.ProductID = e.T03_2X1" +
                    " join S02 on S02_X1<>A1.ProductID";

                if (proDucttype == "2")  //ขายดี
                { query += " left outer join [000_Bestseller] Best on Best.ProductID=A1.ProductID "; }

                query += " where( (M01_X37 = '0' or M01_X37 = '2' or M01_X37 = '1') and(M01_X40 = '0' or M01_X40 = '2')  " +
                "       and TypeOfProductCode<>'005' and  M01_X58 = '0')  " +
                " or " +

                " (  ((M01_X37 = '0' or M01_X37 = '2' or M01_X37 = '1') and(M01_X40 = '0' or M01_X40 = '2')   " +
                "   and  M01_X58 = '0' and(TypeOfProductCode = '005'  or TypeOfProductCode = '006')  " +
                " and(d.T03_X5 is null or d.T03_X5 = @Membercode) " +
                "   and M01_X52 > 0 and M01_X52 > COALESCE(TotalbuyPerson, 0)) " +
                "   and " +
                "   (M01_X37 = '0' or M01_X37 = '2' or M01_X37 = '1') and(M01_X40 = '0' or M01_X40 = '2')  " +
                "   and  M01_X58 = '0' and(TypeOfProductCode = '005'  or TypeOfProductCode = '006') " +
                "   and M01_X53 > 0 and M01_X53 > COALESCE(TotalbuyALL, 0)) " +
                //" ) " +
                "   or " +
                "   ((M01_X37 = '0' or M01_X37 = '2' or M01_X37 = '1') and(M01_X40 = '0' or M01_X40 = '2')  " +
                "   and  M01_X58 = '1' and(TypeOfProductCode = '005'  or TypeOfProductCode = '006')  " +
                " and(d.T03_X5 = @Membercode and @Registerdate between M01_X59 and M01_X60) " +
                "   and M01_X52 > 0 and M01_X52 > COALESCE(TotalbuyPerson, 0)) ";

                if (!string.IsNullOrEmpty(groupCode))
                {
                    query += " and  ProductGroupcode=@groupCode ";
                }
                if (!string.IsNullOrEmpty(productid))
                {
                    query += " and  A1.ProductID=@productid ";
                }


                //Search by group
                if (string.IsNullOrEmpty(proDucttype))
                {

                }
                else
                {
                    //if (proDucttype == "0")  //ทั้งหมด
                    //{ query += " order by ProductID desc "; }
                    if (proDucttype == "1")  //โปรโมชั่น
                    { query += " and TypeOfProductCode='005' or TypeOfProductCode='006' "; }
                    if (proDucttype == "2")  //ขายดี
                    { query += " and  Best.ProductID=A1.ProductID "; }
                    if (proDucttype == "3")  //มาใหม่
                    { query += " and TypeOfProductCode<>'005' and TypeOfProductCode<>'006' and FORMAT( up_date, 'yyyy-MM') = FORMAT( GETDATE(), 'yyyy-MM') "; }


                }

                //Sort
                if (string.IsNullOrEmpty(sortOrder))
                {
                    query += " order by A1.ProductID desc ";
                }
                else
                {
                    if (sortOrder == "0")
                    { query += " order by A1.ProductID desc "; }
                    if (sortOrder == "1")
                    { query += " order by A1.ProductID asc "; }
                    if (sortOrder == "2")
                    { query += " order by MemberPrice asc "; }
                    if (sortOrder == "3")
                    { query += " order by MemberPrice desc "; }
                    if (sortOrder == "4")
                    { query += " order by PV asc "; }
                    if (sortOrder == "5")
                    { query += " order by PV desc "; }
                    if (proDucttype == "3")  //มาใหม่
                    { query += " order by A1.ProductID desc "; }
                }
                // End Sort
                using var command = new SqlCommand(query, con);
                command.Parameters.AddWithValue("@Membercode", memberCode);
                command.Parameters.AddWithValue("@Registerdate", Registerdate);



                if (!string.IsNullOrEmpty(groupCode))
                {
                    command.Parameters.AddWithValue("@groupCode", groupCode);
                }

                if (!string.IsNullOrEmpty(productid))
                {
                    command.Parameters.AddWithValue("@productid", productid);
                }

                using var reader = await command.ExecuteReaderAsync();
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
            catch (Exception)
            {
                // Log exception
                return new List<dynamic> { new { Membercode = "", Error = "An error occurred while fetching data" } };
            }

            return result.Count > 0 ? result : new List<dynamic> { new { Membercode = "" } };
        }
    }
}
