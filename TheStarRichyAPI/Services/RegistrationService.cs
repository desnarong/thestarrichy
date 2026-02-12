using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text.Json;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    /// <summary>
    /// Service implementation สำหรับการลงทะเบียนสมาชิก
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        private readonly string _connectionString;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(IConfiguration configuration, ILogger<RegistrationService> logger)
        {
            _connectionString = configuration.GetConnectionString("MLMConnectionString") 
                ?? throw new InvalidOperationException("Connection string 'MLMConnectionString' not found.");
            _logger = logger;
        }

        /// <summary>
        /// ลงทะเบียนสมาชิกแบบง่าย (Easy Registration)
        /// </summary>
        public async Task<RegistrationResponse> EasyRegisterAsync(EasyRegistrationRequest request, string? currentMemberCode = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_SyncMemberToM06", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CountryCode", (object)NormalizeString(request.Country) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessCountryCode", DBNull.Value);
                        command.Parameters.AddWithValue("@Sponsercode", (object)NormalizeString(request.ReferrerCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Uplinecode", DBNull.Value);
                        command.Parameters.AddWithValue("@Side", DBNull.Value);
                        command.Parameters.AddWithValue("@Registerdate", DateTime.Now.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@birthDate", DBNull.Value);
                        command.Parameters.AddWithValue("@NameTitle", (object)NormalizeString(request.Title) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IdcardName", (object)NormalizeString(request.FullName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessName", DBNull.Value);
                        command.Parameters.AddWithValue("@IDCard", (object)NormalizeString(request.DocumentNumber) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@MobilePhone", (object)NormalizeString(request.Mobile) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Homephone", DBNull.Value);
                        command.Parameters.AddWithValue("@Email", (object)NormalizeString(request.Email) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@lineID", (object)NormalizeString(request.LineId) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Membertype", DBNull.Value);
                        command.Parameters.AddWithValue("@Sex", (object)MapSexFromTitle(request.Title) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Lang", DBNull.Value);
                        command.Parameters.AddWithValue("@SMS", DBNull.Value);
                        command.Parameters.AddWithValue("@Persontype", DBNull.Value);
                        command.Parameters.AddWithValue("@Maritalstatus", DBNull.Value);
                        command.Parameters.AddWithValue("@Spousename", DBNull.Value);
                        command.Parameters.AddWithValue("@Bankcode", DBNull.Value);
                        command.Parameters.AddWithValue("@Bankaccountnumber", DBNull.Value);
                        command.Parameters.AddWithValue("@Bankaccountname", DBNull.Value);
                        command.Parameters.AddWithValue("@Bankbranch", DBNull.Value);
                        command.Parameters.AddWithValue("@beneficiary", DBNull.Value);
                        command.Parameters.AddWithValue("@beneficiaryidcode", DBNull.Value);
                        command.Parameters.AddWithValue("@idaddress", (object)NormalizeString(request.AddressIdCard) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@idaddress_province", (object)NormalizeString(request.ProvinceCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@idaddress_zipcode", (object)NormalizeString(request.Postcode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Presentaddress", (object)NormalizeString(request.AddressIdCard) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Presentaddress_province", (object)NormalizeString(request.ProvinceCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Presentaddress_zipcode", (object)NormalizeString(request.Postcode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@memberpic", DBNull.Value);
                        command.Parameters.AddWithValue("@Createby", (object)NormalizeString(currentMemberCode) ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                _logger.LogInformation("Easy registration successful for {DocumentNumber}", request.DocumentNumber);

                return new RegistrationResponse
                {
                    Success = true,
                    Message = "ลงทะเบียนสำเร็จ",
                    MemberName = request.FullName,
                    RegistrationDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during easy registration for {DocumentNumber}", request.DocumentNumber);
                return new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการลงทะเบียน: " + ex.Message
                };
            }
        }

        /// <summary>
        /// ลงทะเบียนสมาชิกแบบเต็ม (Full Registration)
        /// </summary>
        public async Task<RegistrationResponse> FullRegisterAsync(FullRegistrationRequest request, string? currentMemberCode = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_SyncMemberToM06", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CountryCode", (object)NormalizeString(request.Country) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessCountryCode", (object)NormalizeString(request.BusinessCountry) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Sponsercode", (object)NormalizeString(request.ReferrerCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Uplinecode", (object)NormalizeString(request.UplineCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Side", (object)MapSide(request.UplineSide ?? request.ReferrerSide) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Registerdate", DateTime.Now.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@birthDate", (object)ToDateString(request.DateOfBirth) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NameTitle", (object)NormalizeString(request.Title) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IdcardName", (object)NormalizeString(request.NameOnCard) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessName", (object)NormalizeString(request.BusinessName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDCard", (object)NormalizeString(request.CitizenNumber) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@MobilePhone", (object)NormalizeString(request.Mobile) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Homephone", (object)NormalizeString(request.HomePhone) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Email", (object)NormalizeString(request.Email) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@lineID", (object)NormalizeString(request.LineId) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Membertype", (object)MapMemberType(request.BusinessType) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Sex", (object)MapSexFromTitle(request.Title) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Lang", DBNull.Value);
                        command.Parameters.AddWithValue("@SMS", DBNull.Value);
                        command.Parameters.AddWithValue("@Persontype", DBNull.Value);
                        command.Parameters.AddWithValue("@Maritalstatus", DBNull.Value);
                        command.Parameters.AddWithValue("@Spousename", DBNull.Value);
                        command.Parameters.AddWithValue("@Bankcode", (object)NormalizeString(request.BankCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Bankaccountnumber", (object)NormalizeString(request.BankAccountNumber) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Bankaccountname", (object)NormalizeString(request.BankAccountName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Bankbranch", (object)NormalizeString(request.BankBranch) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@beneficiary", DBNull.Value);
                        command.Parameters.AddWithValue("@beneficiaryidcode", DBNull.Value);
                        command.Parameters.AddWithValue("@idaddress", (object)NormalizeString(request.AddressIdCard) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@idaddress_province", (object)NormalizeString(request.ProvinceCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@idaddress_zipcode", (object)NormalizeString(request.Postcode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Presentaddress", (object)NormalizeString(GetPresentAddress(request)) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Presentaddress_province", (object)NormalizeString(GetPresentProvince(request)) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Presentaddress_zipcode", (object)NormalizeString(GetPresentZipcode(request)) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@memberpic", (object)BuildMemberPicJson(request) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Createby", (object)NormalizeString(currentMemberCode) ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                _logger.LogInformation("Full registration successful for {CitizenNumber}", request.CitizenNumber);

                return new RegistrationResponse
                {
                    Success = true,
                    Message = "ลงทะเบียนสำเร็จ",
                    MemberName = request.NameOnCard,
                    RegistrationDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during full registration for {CitizenNumber}", request.CitizenNumber);
                return new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการลงทะเบียน: " + ex.Message
                };
            }
        }

        /// <summary>
        /// ลงทะเบียนจากภายนอก (External Registration - ไม่ต้อง login)
        /// ใช้ logic เดียวกับ Easy Registration
        /// </summary>
        public async Task<RegistrationResponse> ExternalRegisterAsync(ExternalRegistrationRequest request)
        {
            try
            {
                // Convert to EasyRegistrationRequest and call EasyRegisterAsync
                var easyRequest = new EasyRegistrationRequest
                {
                    Country = request.Country,
                    DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber,
                    ReferrerCode = request.ReferrerCode,
                    Title = request.Title,
                    FullName = request.FullName,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    LineId = request.LineId,
                    AddressIdCard = request.AddressIdCard,
                    Postcode = request.Postcode,
                    ProvinceCode = request.ProvinceCode,
                    DistrictCode = request.DistrictCode,
                    SubdistrictCode = request.SubdistrictCode,
                    VerificationMethod = request.VerificationMethod
                };

                _logger.LogInformation("External registration from source: {Source}, campaign: {Campaign}", 
                    request.SourcePage, request.CampaignCode);

                // Call without current member code (external registration)
                return await EasyRegisterAsync(easyRequest, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during external registration for {DocumentNumber}", request.DocumentNumber);
                return new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการลงทะเบียน: " + ex.Message
                };
            }
        }

        private static string? ToDateString(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : null;
        }

        private static string? MapSide(string? side)
        {
            if (string.IsNullOrWhiteSpace(side))
            {
                return null;
            }

            var normalized = side.Trim().ToLowerInvariant();
            if (normalized == "1" || normalized == "right")
            {
                return "1";
            }

            return "0";
        }

        private static string? MapMemberType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }

            var normalized = type.Trim().ToLowerInvariant();
            return normalized == "user" || normalized == "00" ? "00" : "01";
        }

        private static string? MapSexFromTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var normalized = title.Trim().ToLowerInvariant();
            if (normalized == "mrs" || normalized == "miss" || normalized == "ms" || normalized == "female")
            {
                return "1";
            }

            return "0";
        }

        private static string? GetPresentAddress(FullRegistrationRequest request)
        {
            if (request.UseIdCardAddress || string.IsNullOrWhiteSpace(request.CurrentAddress))
            {
                return request.AddressIdCard;
            }

            return request.CurrentAddress;
        }

        private static string? GetPresentProvince(FullRegistrationRequest request)
        {
            if (request.UseIdCardAddress || string.IsNullOrWhiteSpace(request.CurrentProvinceCode))
            {
                return request.ProvinceCode;
            }

            return request.CurrentProvinceCode;
        }

        private static string? GetPresentZipcode(FullRegistrationRequest request)
        {
            if (request.UseIdCardAddress || string.IsNullOrWhiteSpace(request.CurrentPostcode))
            {
                return request.Postcode;
            }

            return request.CurrentPostcode;
        }

        private static string? BuildMemberPicJson(FullRegistrationRequest request)
        {
            var pics = new List<string>();

            var idCardFront = request.IdCardImageFront;
            var idCardBack = request.IdCardImageBack;
            if (!string.IsNullOrWhiteSpace(idCardFront))
            {
                pics.Add(idCardFront);
            }
            else if (!string.IsNullOrWhiteSpace(idCardBack))
            {
                pics.Add(idCardBack);
            }

            if (!string.IsNullOrWhiteSpace(request.BankBookImage))
            {
                pics.Add(request.BankBookImage);
            }

            if (!string.IsNullOrWhiteSpace(request.ProfileImage))
            {
                pics.Add(request.ProfileImage);
            }

            return pics.Count == 0 ? null : JsonSerializer.Serialize(pics);
        }

        private static string? NormalizeString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        /// <summary>
        /// ค้นหาข้อมูลผู้อ้างอิง
        /// </summary>
        public async Task<FindReferrerResponse> FindReferrerAsync(string referrerCode)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = @"SELECT TOP 1 M06_PX1, M06_X5, M06_X34 FROM M06 WHERE M06_PX1 = @ReferrerCode AND memberflag = 'Y' AND activeflag = 'Y'";
                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@ReferrerCode", referrerCode);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    string memberCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0); // [M06_PX1]
                                    string memberName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1); // [M06_X5]
                                    string side = reader.IsDBNull(2) ? string.Empty : reader.GetString(2); // [M06_X34]
                                    // สามารถเพิ่ม Side ใน response ได้ถ้าต้องการ
                                    return new FindReferrerResponse
                                    {
                                        Success = true,
                                        MemberCode = memberCode,
                                        MemberName = memberName,
                                        Message = "พบข้อมูลผู้อ้างอิง",
                                        // เพิ่ม property Side ถ้ามีใน DTO
                                        // Side = side
                                    };
                                }
                            }
                        }
                    }
                }
                return new FindReferrerResponse
                {
                    Success = false,
                    Message = "ไม่พบข้อมูลผู้อ้างอิง"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding referrer {ReferrerCode}", referrerCode);
                return new FindReferrerResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการค้นหาข้อมูล: " + ex.Message
                };
            }
        }

        /// <summary>
        /// ตรวจสอบว่าเลขบัตรประชาชน/เอกสารซ้ำหรือไม่
        /// </summary>
        public async Task<bool> IsDocumentNumberExistsAsync(string IDCard)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = @"SELECT COUNT(1) FROM M06 WHERE M06_X10 = @IDCard";
                    using (var command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@IDCard", IDCard);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            return count > 0;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking document number {IDCard}", IDCard);
                return false;
            }
        }
    }
}
