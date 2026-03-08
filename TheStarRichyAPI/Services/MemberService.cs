using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Security.Claims;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    public interface IMemberService
    {
        Task<string> EncodeAsync(string password);
        Task<string> DecodeAsync(string password);
        Task<string> GetPasskeyAsync(string column);
        Task<List<GetInfoMember2>> GetMember2DisplayAsync(string baseUrl);
        Task<UpdateMemberProfileResult> UpdateMemberProfileAsync(UpdateMemberProfileRequest request);
        Task<UpdateMemberProfileResult> UpdateMemberProfilePic4Async(string profileImageUrl);
    }
    public class MemberService : IMemberService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> DecodeAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "";

            string fieldCont = password.Trim();
            string newField = "";
            int position = 0;

            while (position < fieldCont.Length)
            {
                char mChar = fieldCont[position];
                newField += (char)(mChar - 3);
                position++;
            }

            newField = newField.Substring(4);
            newField = newField.Substring(0, newField.Length - 5);

            return newField;
        }

        public async Task<string> EncodeAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "";

            string fieldCont = password.Trim();
            string newField = "";
            int position = 0;

            while (position < fieldCont.Length)
            {
                char mChar = fieldCont[position];
                newField += (char)(mChar + 3);
                position++;
            }

            return $"123{(char)50}{newField}{(char)50}!!!!";
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

        private string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return "";

            if (DateTime.TryParse(dateObj.ToString(), out DateTime date))
            {
                return date.ToString("dd/MM/yyyy");
            }
            return "";
        }

        public async Task<List<GetInfoMember2>> GetMember2DisplayAsync(string baseUrl)
        {
            // Get Passkey from header
            string passkey = _httpContextAccessor.HttpContext.Request.Headers["X-Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                var errorResult = new List<GetInfoMember2> { new GetInfoMember2 { MemberFlag = "N" } };
                return errorResult;
            }

            string passwordEncode1 = await GetPasskeyAsync("Passkey1");
            string passwordEncode2 = await GetPasskeyAsync("Passkey2");

            var result = new List<GetInfoMember2>();
            var memberInfo = new GetInfoMember2();

            // Verify Passkey using original Encode/Decode
            if (passkey != passwordEncode1 && passkey != passwordEncode2)
            {
                memberInfo.MemberFlag = "N";
                result.Add(memberInfo);
                return result;
            }

            // Get Membercode from JWT
            string memberCode = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberCode))
            {
                memberInfo.MemberFlag = "N";
                result.Add(memberInfo);
                return result;
            }

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT * FROM [000_Member_info_for_mobile] WHERE Membercode = @Membercode";

                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@Membercode", memberCode);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    string memberType = reader["MemberType"]?.ToString() ?? "";
                                    string memberTypeCode = reader["MemberTypeCode"]?.ToString() ?? "";
                                    string flagExpire = reader["FlagExpire"]?.ToString() ?? "";
                                    string memberStatus = reader["MemberStatus"]?.ToString() ?? "";
                                    string m06X9 = reader["M06_X9"]?.ToString() ?? "";
                                    string m06X65 = reader["M06_X65"]?.ToString() ?? "";
                                    string m06X96 = reader["M06_X96"]?.ToString() ?? "";
                                    string m06X50 = reader["M06_X50"]?.ToString() ?? "";
                                    string position = reader["Position1"]?.ToString() ?? "0";
                                    string prestigeRanking = reader["PrestigeRanking"]?.ToString() ?? "";
                                    string ssex = reader["Sex"]?.ToString() ?? "";



                                    memberInfo = new GetInfoMember2
                                    {
                                        MemberType = memberType,
                                        MemberTypeCode = memberTypeCode,
                                        Membercode = memberCode,
                                        Title = reader["Title"]?.ToString() ?? "",
                                        RegisterDate = FormatDate(reader["RegisterDate"]),
                                        BussinessName = reader["BussinessName"]?.ToString() ?? "",
                                        Idcardname = reader["Idcardname"]?.ToString() ?? "",
                                        DateofBirth = FormatDate(reader["DateofBirth"]),
                                        Sex = reader["Sex"]?.ToString() ?? "",
                                        FlagsendIDCard = reader["FlagsendIDCard"]?.ToString() ?? "",
                                        FlagsendBookbank = reader["FlagsendBookbank"]?.ToString() ?? "",
                                        Typeofperson = reader["Typeofperson"]?.ToString() ?? "",
                                        typeofcompany = reader["typeofcompany"]?.ToString() ?? "",
                                        ExpireDate = FormatDate(reader["ExpireDate"]),
                                        FlagExpire = flagExpire,
                                        Lang = reader["Lang"]?.ToString() ?? "",
                                        MemberStatus = memberStatus,
                                        SMSType = reader["SMSType"]?.ToString() ?? "",
                                        MarryStatus = reader["MarryStatus"]?.ToString() ?? "",
                                        MarryName = reader["MarryName"]?.ToString() ?? "",
                                        CreateBy = reader["CreateBy"]?.ToString() ?? "",
                                        CreateDate = FormatDate(reader["CreateDate"]),
                                        Ewallet = reader["Ewallet"]?.ToString() ?? "",
                                        Registervalue = reader["Registervalue"]?.ToString() ?? "",
                                        PositionPromoteDate = FormatDate(reader["PositionPromoteDate"]),
                                        HoldPV = reader["HoldPV"]?.ToString() ?? "",
                                        QualifyDate = FormatDate(reader["QualifyDate"]),
                                        Position1 = position,
                                        PositionName = reader["PositionName"]?.ToString() ?? "",
                                        PrestigeRanking = prestigeRanking,
                                        ThaiName = reader["ThaiName"]?.ToString() ?? "",
                                        Qualifytype = reader["Qualifytype"]?.ToString() ?? "",
                                        PersonalPV = reader["PersonalPV"]?.ToString() ?? "",
                                        Idcard = reader["Idcard"]?.ToString() ?? "",
                                        PresentAddress = reader["PresentAddress"]?.ToString() ?? "",
                                        PresentDistrict = reader["PresentDistrict"]?.ToString() ?? "",
                                        PresentDistrictCode = reader["PresentDistrictCode"]?.ToString() ?? "",
                                        PresentPostcode = reader["PresentPostcode"]?.ToString() ?? "",
                                        IdcardAddress = reader["IdcardAddress"]?.ToString() ?? "",
                                        IDcardPostcode = reader["IDcardPostcode"]?.ToString() ?? "",
                                        IDcardDustrict = reader["IDcardDustrict"]?.ToString() ?? "",
                                        IDcardDustrictCode = reader["IDcardDustrictCode"]?.ToString() ?? "",
                                        CountryCode = reader["CountryCode"]?.ToString() ?? "",
                                        Countryname = reader["Countryname"]?.ToString() ?? "",
                                        LocationCode = reader["LocationCode"]?.ToString() ?? "",
                                        Locationname = reader["Locationname"]?.ToString() ?? "",
                                        Homephone = reader["Homephone"]?.ToString() ?? "",
                                        MobilePhone = reader["MobilePhone"]?.ToString() ?? "",
                                        Email = reader["Email"]?.ToString() ?? "",
                                        CenterID = reader["CenterID"]?.ToString() ?? "",
                                        CenterTelephone = reader["CenterTelephone"]?.ToString() ?? "",
                                        CenterName = reader["CenterName"]?.ToString() ?? "",
                                        CenterPromoteDate = FormatDate(reader["CenterPromoteDate"]),
                                        CenterDistrict = reader["CenterDistrict"]?.ToString() ?? "",
                                        CenterDistrict1 = reader["CenterDistrict1"]?.ToString() ?? "",
                                        SponserCentercode = reader["SponserCentercode"]?.ToString() ?? "",
                                        Bookbankname = reader["Bookbankname"]?.ToString() ?? "",
                                        Bankname = reader["Bankname"]?.ToString() ?? "",
                                        Accountnumber = reader["Accountnumber"]?.ToString() ?? "",
                                        Branchname = reader["Branchname"]?.ToString() ?? "",
                                        Beneficiary = reader["Beneficiary"]?.ToString() ?? "",
                                        BeneficiaryIDCard = reader["BeneficiaryIDCard"]?.ToString() ?? "",
                                        Sponsercode = reader["Sponsercode"]?.ToString() ?? "",
                                        Sponsername = reader["Sponsername"]?.ToString() ?? "",
                                        UplineCode = reader["UplineCode"]?.ToString() ?? "",
                                        UplineName = reader["UplineName"]?.ToString() ?? "",
                                        Side = reader["Side"]?.ToString() ?? "",
                                        Leftcode = reader["Leftcode"]?.ToString() ?? "",
                                        LeftName = reader["LeftName"]?.ToString() ?? "",
                                        LeftPosition = reader["LeftPosition"]?.ToString() ?? "",
                                        LeftPrestigeRanking = reader["LeftPrestigeRanking"]?.ToString() ?? "",
                                        Rightcode = reader["Rightcode"]?.ToString() ?? "",
                                        RightName = reader["RightName"]?.ToString() ?? "",
                                        RightPosition = reader["RightPosition"]?.ToString() ?? "",
                                        RightPrestigeRanking = reader["RightPrestigeRanking"]?.ToString() ?? "",
                                        BWDLeftPV = reader["BWDLeftPV"]?.ToString() ?? "",
                                        BWDRightPV = reader["BWDRightPV"]?.ToString() ?? "",
                                        PresentLeftPV = reader["PresentLeftPV"]?.ToString() ?? "",
                                        PresentRightPV = reader["PresentRightPV"]?.ToString() ?? "",
                                        WeakPV = reader["WeakPV"]?.ToString() ?? "0",
                                        TotalBalance = reader["TotalBalance"]?.ToString() ?? "0",
                                        NewLeftPV = reader["NewLeftPV"]?.ToString() ?? "",
                                        NewRightPV = reader["NewRightPV"]?.ToString() ?? "",
                                        RegisterfromBranch = reader["RegisterfromBranch"]?.ToString() ?? "",
                                        TravelPoint = reader["TravelPoint"]?.ToString() ?? "",
                                        TravelPoint1 = reader["TravelPoint1"]?.ToString() ?? "",
                                        TravelPoint2 = reader["TravelPoint2"]?.ToString() ?? "",
                                        TotalBonus = reader["TotalBonus"]?.ToString() ?? "",
                                        LastBuyDate = FormatDate(reader["LastBuyDate"]),
                                        LastCalcDate = FormatDate(reader["LastCalcDate"]),
                                        CurrentMonthQualifyPV = reader["CurrentMonthQualifyPV"]?.ToString() ?? "",
                                        LastmonthQualifyPV = reader["LastmonthQualifyPV"]?.ToString() ?? "",
                                        NextPosition = reader["NextPosition"]?.ToString() ?? "",
                                        FirstQdate = reader["FirstQdate"]?.ToString() ?? "",
                                        CurrentMonth = reader["CurrentMonth"]?.ToString() ?? "",
                                        NextCMonth = reader["NextCMonth"]?.ToString() ?? "",
                                        CurrentMonth1 = reader["CurrentMonth1"]?.ToString() ?? "",
                                        LastCMonth = reader["LastCMonth"]?.ToString() ?? "",
                                        FlaqSendIDCard = reader["FlaqSendIDCard"]?.ToString() ?? "",
                                        FlaqBookBank = reader["FlaqBookBank"]?.ToString() ?? "",
                                        MemberIDPicture = reader["PIC1"]?.ToString() ?? "",
                                        MemberBookPicture = reader["PIC2"]?.ToString() ?? "",
                                        weakleg = reader["weakleg"]?.ToString() ?? "0",
                                        RunMessage = reader["RunMessage"]?.ToString() ?? "",
                                        Systemname = reader["Systemname"]?.ToString() ?? "",
                                        kyc = reader["kyc"]?.ToString() ?? "",
                                        LeftCountActive = reader["LeftCountActive"]?.ToString() ?? "0",
                                        RightCountActive = reader["RightCountActive"]?.ToString() ?? "0",

                                        TotalLeftBalanceTeam = reader["Total_Left_Sponser_PV"]?.ToString() ?? "0",
                                        TotalRightBalanceTeam = reader["Total_Right_Sponser_PV"]?.ToString() ?? "0",
                                        TotalNewMonthLeftPV = reader["LeftSP"]?.ToString() ?? "0",
                                        TotalNewMonthRightPV = reader["RightSP"]?.ToString() ?? "0",
                                        Totalmember = reader["Totalmember"]?.ToString() ?? "0",
                                        Sumranking = reader["Sumranking"]?.ToString() ?? "0",


                                        Totalmember_buy = reader["Totalmember_buy"]?.ToString() ?? "0",
                                        Totalmember_notbuy = reader["Totalmember_notbuy"]?.ToString() ?? "0",
                                        PVBUY = reader["PVBUY"]?.ToString() ?? "0",
                                        AmountBUY = reader["AmountBUY"]?.ToString() ?? "0",
                                        PVBUY_Month = reader["PVBUY_Month"]?.ToString() ?? "0",
                                        AmountBUY_Month = reader["AmountBUY_Month"]?.ToString() ?? "0",
                                        Totalmember_buy_month = reader["Totalmember_buy_month"]?.ToString() ?? "0",
                                        LastMonthStatus = reader["LastMonthStatus"]?.ToString() ?? "0",
                                        CurrentMonthStatus = reader["CurrentMonthStatus"]?.ToString() ?? "0"


                                    };

                                    // Calculate weakleg and ESTweakleg
                                    double totalBalance = double.TryParse(memberInfo.TotalBalance, out double tb) ? tb : 0;
                                    if (reader["weakleg"] != null && double.TryParse(reader["weakleg"].ToString(), out double weakLeg))
                                    {
                                        memberInfo.ESTweakleg = (weakLeg - totalBalance).ToString();
                                    }

                                    //if (reader["ESTweakleg"] != null && double.TryParse(reader["ESTweakleg"].ToString(), out double estWeakLeg))
                                    //{
                                    //    memberInfo.ESTweakleg = (estWeakLeg - totalBalance).ToString();
                                    //}

                                    // Set Status based on FlagExpire and MemberStatus
                                    string status = "A";
                                    if (flagExpire == "Normal") status = "A";
                                    else if (flagExpire == "Expire") status = "E";
                                    else if (flagExpire == "Terminate") status = "T";

                                    if (memberStatus == "Normal") status = "A";
                                    else if (memberStatus == "Hold") status = "I";
                                    else if (memberStatus == "Resign") status = "R";

                                    memberInfo.Status = status;

                                    // Set MemberPositionPicture

                                    string pictureBase = "";
                                    if (reader["MemberPositionPicture"] != null)
                                    {
                                        pictureBase = reader["MemberPositionPicture"]?.ToString() ?? "";
                                        memberInfo.MemberPositionPicture = pictureBase;
                                    }

                                    string mpic = reader["Pic4"]?.ToString() ?? "";
                                    if (mpic != "")
                                    {
                                        memberInfo.MemberPicture = reader["Pic4"]?.ToString() ?? "";
                                    }
                                    else
                                    {
                                        memberInfo.MemberPicture = pictureBase;
                                    }

                                    //else
                                    //{
                                    //    string suffix1 = ".gif";
                                    //    if (ssex == "FeMale") suffix1 = "_F.gif";
                                    //    if (m06X9 == "0" || m06X65 == "0") suffix1 = "_o_g" + suffix1;


                                    //    memberInfo.MemberPicture = $"{pictureBase}{suffix1}";
                                    //}
                                    // Set LeftURL and RightURL
                                    string url = "";
                                    if (reader["webappPath"] != null)
                                    {
                                        url = reader["webappPath"]?.ToString() ?? "";
                                    }
                                    url = $"{url}?id={memberCode}";

                                    //string url = baseUrl.EndsWith("/") ? $"{baseUrl}Member_register3.aspx?id={memberCode}" : $"{baseUrl}/Member_register3.aspx?id={memberCode}";
                                    memberInfo.LeftURL = $"{url}L";
                                    memberInfo.RightURL = $"{url}R";

                                    // Set MemberFlag
                                    if (status == "T" || status == "I" || status == "R")
                                    {
                                        memberInfo = new GetInfoMember2 { Status = status, MemberFlag = "N" };
                                    }
                                    else
                                    {
                                        memberInfo.MemberFlag = "Y";
                                    }
                                }
                            }
                            else
                            {
                                memberInfo = new GetInfoMember2 { MemberFlag = "N" };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log exception in production
                throw;
            }

            result.Add(memberInfo);
            return result;
        }

        public Task<UpdateMemberProfileResult> UpdateMemberProfileAsync(UpdateMemberProfileRequest request)
        {
            string memberCode = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(memberCode))
            {
                return Task.FromResult(new UpdateMemberProfileResult
                {
                    Success = false,
                    Message = "ไม่พบข้อมูลสมาชิกจาก Token"
                });
            }

            if (request == null)
            {
                return Task.FromResult(new UpdateMemberProfileResult
                {
                    Success = false,
                    Message = "ไม่พบข้อมูลสำหรับอัปเดต"
                });
            }

            return UpdateMemberProfileInternalAsync(memberCode, request);
        }

        public async Task<UpdateMemberProfileResult> UpdateMemberProfilePic4Async(string profileImageUrl)
        {
            string memberCode = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(memberCode))
            {
                return new UpdateMemberProfileResult
                {
                    Success = false,
                    Message = "ไม่พบข้อมูลสมาชิกจาก Token"
                };
            }

            if (string.IsNullOrWhiteSpace(profileImageUrl))
            {
                return new UpdateMemberProfileResult
                {
                    Success = false,
                    Message = "ไม่พบ URL รูปโปรไฟล์สำหรับอัปเดต"
                };
            }

            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();

                const string sql = @"
UPDATE M06
SET PIC4 = @Pic4
WHERE M06_PX1 = @Membercode
  AND M06_X47 = N'0';";

                using var command = new SqlCommand(sql, con);
                command.Parameters.AddWithValue("@Membercode", memberCode);
                command.Parameters.AddWithValue("@Pic4", ToDbValue(profileImageUrl));

                int rows = await command.ExecuteNonQueryAsync();
                if (rows <= 0)
                {
                    return new UpdateMemberProfileResult
                    {
                        Success = false,
                        Message = "ไม่พบข้อมูลสมาชิกสำหรับอัปเดตรูปโปรไฟล์"
                    };
                }

                return new UpdateMemberProfileResult
                {
                    Success = true,
                    Message = "อัปเดตรูปโปรไฟล์เรียบร้อย"
                };
            }
            catch (Exception ex)
            {
                return new UpdateMemberProfileResult
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาดในการอัปเดตรูปโปรไฟล์: {ex.Message}"
                };
            }
        }

        private async Task<UpdateMemberProfileResult> UpdateMemberProfileInternalAsync(string memberCode, UpdateMemberProfileRequest request)
        {
            string connectionString = _configuration.GetConnectionString("MLMConnectionString");

            try
            {
                using var con = new SqlConnection(connectionString);
                await con.OpenAsync();

                const string sql = @"
UPDATE M06
SET
    M06_X5 = COALESCE(@BussinessName, M06_X5),
    M06_X11 = COALESCE(@PresentAddress, M06_X11),
    M06_X12 = COALESCE(@IdcardAddress, M06_X12),
    M06_X25 = COALESCE(@Bookbankname, M06_X25),
    M06_X27 = COALESCE(@Accountnumber, M06_X27),
    M06_X28 = COALESCE(@Branchname, M06_X28),
    PIC1 = COALESCE(@Pic1, PIC1),
    PIC2 = COALESCE(@Pic2, PIC2),
    PIC3 = COALESCE(@Pic3, PIC3),
    PIC4 = COALESCE(@Pic4, PIC4)
WHERE M06_PX1 = @Membercode
  AND M06_X47 = N'0';";

                using var command = new SqlCommand(sql, con);
                command.Parameters.AddWithValue("@Membercode", memberCode);
                command.Parameters.AddWithValue("@BussinessName", ToDbValue(request.PersonalInfo?.BussinessName));
                command.Parameters.AddWithValue("@PresentAddress", ToDbValue(request.AddressInfo?.PresentAddress?.AddressLine));
                command.Parameters.AddWithValue("@IdcardAddress", ToDbValue(request.AddressInfo?.IdCardAddress?.AddressLine));
                command.Parameters.AddWithValue("@Bookbankname", ToDbValue(request.BankInfo?.AccountName));
                command.Parameters.AddWithValue("@Accountnumber", ToDbValue(request.BankInfo?.AccountNumber));
                command.Parameters.AddWithValue("@Branchname", ToDbValue(request.BankInfo?.BranchName));
                command.Parameters.AddWithValue("@Pic1", ToDbValue(request.DocumentInfo?.IdCardImageUrl));
                command.Parameters.AddWithValue("@Pic2", ToDbValue(request.DocumentInfo?.BankBookImageUrl));
                command.Parameters.AddWithValue("@Pic3", ToDbValue(request.DocumentInfo?.ApplicationFormImageUrl));
                command.Parameters.AddWithValue("@Pic4", ToDbValue(request.DocumentInfo?.ProfileImageUrl));

                int rows = await command.ExecuteNonQueryAsync();
                if (rows <= 0)
                {
                    return new UpdateMemberProfileResult
                    {
                        Success = false,
                        Message = "ไม่พบข้อมูลสมาชิกสำหรับอัปเดต"
                    };
                }

                return new UpdateMemberProfileResult
                {
                    Success = true,
                    Message = "อัปเดตข้อมูลสมาชิกเรียบร้อย"
                };
            }
            catch (Exception ex)
            {
                return new UpdateMemberProfileResult
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาดในการอัปเดตข้อมูล: {ex.Message}"
                };
            }
        }

        private static object ToDbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }
    }
    public class GetInfoMember2
    {
        public string Membercode { get; set; } = "";
        public string Title { get; set; } = "";
        public string RegisterDate { get; set; } = "";
        public string BussinessName { get; set; } = "";
        public string Idcardname { get; set; } = "";
        public string DateofBirth { get; set; } = "";
        public string Sex { get; set; } = "";
        public string FlagsendIDCard { get; set; } = "";
        public string FlagsendBookbank { get; set; } = "";
        public string Typeofperson { get; set; } = "";
        public string typeofcompany { get; set; } = "";
        public string ExpireDate { get; set; } = "";
        public string FlagExpire { get; set; } = "";
        public string Lang { get; set; } = "";
        public string MemberStatus { get; set; } = "";
        public string SMSType { get; set; } = "";
        public string MemberType { get; set; } = "";
        public string MemberTypeCode { get; set; } = "";
        public string MarryStatus { get; set; } = "";
        public string MarryName { get; set; } = "";
        public string CreateBy { get; set; } = "";
        public string CreateDate { get; set; } = "";
        public string Ewallet { get; set; } = "";
        public string Registervalue { get; set; } = "";
        public string PositionPromoteDate { get; set; } = "";
        public string HoldPV { get; set; } = "";
        public string QualifyDate { get; set; } = "";
        public string Position1 { get; set; } = "";
        public string PositionName { get; set; } = "";
        public string PrestigeRanking { get; set; } = "";
        public string ThaiName { get; set; } = "";
        public string Qualifytype { get; set; } = "";
        public string PersonalPV { get; set; } = "";
        public string Idcard { get; set; } = "";
        public string PresentAddress { get; set; } = "";
        public string PresentDistrict { get; set; } = "";
        public string PresentDistrictCode { get; set; } = "";
        public string PresentPostcode { get; set; } = "";
        public string IdcardAddress { get; set; } = "";
        public string IDcardPostcode { get; set; } = "";
        public string IDcardDustrict { get; set; } = "";
        public string IDcardDustrictCode { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public string Countryname { get; set; } = "";
        public string LocationCode { get; set; } = "";
        public string Locationname { get; set; } = "";
        public string Homephone { get; set; } = "";
        public string MobilePhone { get; set; } = "";
        public string Email { get; set; } = "";
        public string CenterID { get; set; } = "";
        public string CenterTelephone { get; set; } = "";
        public string CenterName { get; set; } = "";
        public string CenterPromoteDate { get; set; } = "";
        public string CenterDistrict { get; set; } = "";
        public string CenterDistrict1 { get; set; } = "";
        public string SponserCentercode { get; set; } = "";
        public string Bookbankname { get; set; } = "";
        public string Bankname { get; set; } = "";
        public string Accountnumber { get; set; } = "";
        public string Branchname { get; set; } = "";
        public string Beneficiary { get; set; } = "";
        public string BeneficiaryIDCard { get; set; } = "";
        public string Sponsercode { get; set; } = "";
        public string Sponsername { get; set; } = "";
        public string UplineCode { get; set; } = "";
        public string UplineName { get; set; } = "";
        public string Side { get; set; } = "";
        public string Leftcode { get; set; } = "";
        public string LeftName { get; set; } = "";
        public string LeftPosition { get; set; } = "";
        public string LeftPrestigeRanking { get; set; } = "";
        public string Rightcode { get; set; } = "";
        public string RightName { get; set; } = "";
        public string RightPosition { get; set; } = "";
        public string RightPrestigeRanking { get; set; } = "";
        public string BWDLeftPV { get; set; } = "";
        public string BWDRightPV { get; set; } = "";
        public string PresentLeftPV { get; set; } = "";
        public string PresentRightPV { get; set; } = "";
        public string WeakPV { get; set; } = "";
        public string TotalBalance { get; set; } = "";
        public string NewLeftPV { get; set; } = "";
        public string NewRightPV { get; set; } = "";
        public string RegisterfromBranch { get; set; } = "";
        public string TravelPoint { get; set; } = "";
        public string TravelPoint1 { get; set; } = "";
        public string TravelPoint2 { get; set; } = "";
        public string TotalBonus { get; set; } = "";
        public string LastBuyDate { get; set; } = "";
        public string LastCalcDate { get; set; } = "";
        public string CurrentMonthQualifyPV { get; set; } = "";
        public string LastmonthQualifyPV { get; set; } = "";
        public string Status { get; set; } = "";
        public string MemberFlag { get; set; } = "";
        public string MemberPicture { get; set; } = "";
        public string MemberIDPicture { get; set; } = "";
        public string MemberBookPicture { get; set; } = "";
        public string MemberPositionPicture { get; set; } = "";
        public string FlaqSendIDCard { get; set; } = "";
        public string FlaqBookBank { get; set; } = "";
        public string LeftURL { get; set; } = "";
        public string RightURL { get; set; } = "";
        public string weakleg { get; set; } = "";
        public string ESTweakleg { get; set; } = "";
        public string NextPosition { get; set; } = "";
        public string FirstQdate { get; set; } = "";
        public string CurrentMonth { get; set; } = "";
        public string NextCMonth { get; set; } = "";
        public string CurrentMonth1 { get; set; } = "";
        public string LastCMonth { get; set; } = "";
        public string TotalLeftBalanceTeam { get; set; } = "";
        public string TotalRightBalanceTeam { get; set; } = "";
        public string TotalNewMonthLeftPV { get; set; } = "";
        public string TotalNewMonthRightPV { get; set; } = "";
        public string RunMessage { get; set; } = "";
        public string Systemname { get; set; } = "";
        public string LeftCountActive { get; set; } = "";
        public string RightCountActive { get; set; } = "";
        public string Totalmember { get; set; } = "";
        public string Totalmember_buy { get; set; } = "";
        public string Totalmember_notbuy { get; set; } = "";
        public string PVBUY { get; set; } = "";
        public string AmountBUY { get; set; } = "";
        public string PVBUY_Month { get; set; } = "";
        public string AmountBUY_Month { get; set; } = "";
        public string Totalmember_buy_month { get; set; } = "";
        public string Sumranking { get; set; } = "";

        public string LastMonthStatus { get; set; } = "";

        public string CurrentMonthStatus { get; set; } = "";

        public string kyc { get; set; } = "";









    }
}
