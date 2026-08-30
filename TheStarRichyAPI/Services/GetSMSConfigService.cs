using System.Data.SqlClient;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace TheStarRichyApi.Services
{
    public interface IGetSMSConfigService
    {
        Task<List<dynamic>> GetDisplayAsync();
    }
    public class GetSMSConfigService : IGetSMSConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetSMSConfigService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<dynamic>> GetDisplayAsync()
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

            var result = new List<dynamic>();
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    /*
                     * 
                     * ให้ load API นี้ตอนเข้าหน้าสมัคร
                    MinPerson - S02_X74  จำนวนรหัสที่สมัครได้สูงสุด/คน;
                    Maxdate = S02_X90  จำนวนวันที่ลาออกและสมัครใหม่ได้;
                    FirstDigitOfMembercode - S02_X94  อักษรแรกของรหัสสมาชิก;
                    FlagRuncode = S02_X93  Flag รันรหัส 0 - เรียงเลข 1 - สุ่มเลข;
                    FlagForceUpload=S02_X101  Flag บังคับ upload เอกสาร 0-ไม่ 1-ต้อง upload;
                    FlagSendSMS- S02_X132- ส่ง SMS แจ้งการสมัครสมาชิก 0-ส่ง 1-ไม่ส่ง
                    MinAge - S02_X139 อายุขั้นต่ำที่สามารถสมัครได้;
                    FlagOtpConfirm - S02_X47- ส่ง SMS OTP  'Y' -  'N'-ไม่ส่ง
                    */
                    /*
                    การสมัครสมาชิก-เมื่อกดบันทึก
                    1. เมื่อกด บันทึก ให้มี popup ยืนยัน-ยกเลิก  
2. ถ้ากดยืนยัน ให้ทำการ validate ข้อมูลดังนี้
3. เรียก Query CheckBlacklist  key IDCardnumber  ถ้าค้นหาข้อมูลพบ จะไม่ให้ผ่าน   ให้แจ้งเตือนว่าเลขที่บัตรนี้ไม่สามารถสมัครได้ในขณะนี้ หากมีข้อสงสัยกรุณาติดต่อบริษัท    ถ้าไม่มีข้อมูลให้ผ่าน
4..เรียก Query Checkexpire - ตรวจรหัสหมดอายุหรือยัง  โดยใช้เลขที่บัตรประชาชนค้นหา  key IDCardnumber  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า เลขที่บัตรนี้หมดอายุยังไม่ครบ MaxexpireDate วัน  ถ้าไม่มีข้อมูลให้ผ่าน
5.เรียก Query CheckMemberResign - ตรวจรหัสลาออกครบวัน หรือยัง  โดยใช้เลขที่บัตรประชาชนค้นหา  key IDCardnumber  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า เลขที่บัตรนี้ลาออกยังไม่ครบ MaxexpireDate วัน    ถ้าไม่มีข้อมูลให้ผ่าน
6. เรียก Query CheckSponserCode - ตรวจรหัสผู้แนะนำว่ามีปัญหาหรือไม่  โดยใช้รหัสผู้แนะนำค้นหา  key Membercode  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า  รหัสผู้แนะนำไม่สามารถใช้งานได้กรุณาติดต่อผู้แนะนำ    ถ้าไม่มีข้อมูลให้ผ่าน
7. เรียก Query CheckDupIDcard - ตรวจรหัสเลขที่บัตรซ้ำในระบบหรือไม่  โดยใช้   key IDCardnumber  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า  บัตรประชาชนนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
8. เรียก Query CheckDupIDcardname - ตรวจชื่อตามบัตรซ้ำในระบบหรือไม่  โดยใช้   key IDCardname  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า  ชื่อตามบัตรประชาชนนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
9. เรียก Query CheckDupBusinessname - ตรวจชื่อทางธุรกิจซ้ำในระบบหรือไม่  โดยใช้   key Businessname  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า  ชื่อทางธุรกิจนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
10. เรียก Query CheckDupTelephone - ตรวจเบอร์โทรซ้ำในระบบหรือไม่  โดยใช้   key Telephonenumber  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า  เบอร์โทรศัพท์นี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
11. เรียก Query CheckDupBankAccountNumber - ตรวจเลขที่บัญชีนี้ซ้ำในระบบหรือไม่  โดยใช้   key Bankcode,BankACCountNumber ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า บัญชีธนาคารนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
12. เรียก Query CheckDupBankAccountName - ตรวจชื่อบัญชีนี้ซ้ำในระบบหรือไม่  โดยใช้   key Bankcode,BankACCountName  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า ชื่อบัญชีธนาคารนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
13. ถ้ามีการใส่ email เรียก Query CheckDupEmail - ตรวจ Email ซ้ำในระบบหรือไม่  โดยใช้   key Email  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า Email นี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
14. ถ้ามีการใส่ LineID เรียก Query CheckDupLineid - ตรวจ LineID ซ้ำในระบบหรือไม่  โดยใช้   key LineID  ถ้ามีข้อมูล จะไม่ให้ผ่าน  ให้แจ้ง เตือนว่า LineID นี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง     ถ้าไม่มีข้อมูลให้ผ่าน
15. ตรวจสอบอายุผู้สมัคร    datediff(year,Birthdate,getdate())<MinAge   ถ้าอายุต่ำกว่า Minage ให้แจ้งเตือนว่า อายุผู้สมัครยังไม่ถึง Minage ไม่สามารถสมัครได้   ถ้าไม่มีข้อมูลให้ผ่าน
16. upload file resize ลงด้วย
17..เมื่อ validate ผ่านแล้ว ข้อมูลแล้ว ให้ส่ง OTP ไปที่หมายเลขโทรศัพท์ของผู้สมัคร และมีช่องให้ใส่ OTP ตามภาพ

                    เมือกดยืนยัน OTP ผ่าน
                    1.การค้นหารหัสล่าสุด
                    select top 1 M06_PX1 from M06(nolock)  where M06_X47 = '0' and M06_X14 = '0' Order by  M06_PX1 desc
                    query checklastMembercode
                    2.รูปแบบรหัสสมาชิก  1ตัวอักษรและตัวเลข แปดหลัก รันเลข
                    FirstDigitOfMembercode + Right(M06_PX1, Len(M06_PX1) -Len(FirstDigitOfMembercode));
                    3. select top 1    M24_PX1 from M24  Order by  M24_X8  ตำแหน่งเริ่มต้น
                        query checkfirstposition1 ตำแหน่ง package M06_X59
                        query checkfirstposition2 ตำแหน่ง คุณวุฒิ
                    

                    */


                    string query = "select smsapikey,smsapisecret,smsSMTPmail,smssendername,smsapiSend,smsShortURL,SMSWelcome,SMSWelcomeEng " +
                            ",S02_X132 as FlagSendSMS,S02_X74 as MinPerson,S02_X90 as Maxdate,S02_X94 asFirstDigitOfMembercode "+
                            ",S02_X93 as FlagRuncode,S02_X101 as FlagForceUpload,S02_X132 as FlagSendSMS from S02  (nolock)   ";

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
                                    object columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
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
                return new List<dynamic>();
            }

            return result.Count > 0 ? result : new List<dynamic>();
        }
    }
}
