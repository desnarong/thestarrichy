using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    /// <summary>
    /// Service implementation สำหรับการลงทะเบียนสมาชิก
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationService> _logger;
        private readonly ISMSService _smsService;

        public RegistrationService(IConfiguration configuration, ILogger<RegistrationService> logger, ISMSService smsService)
        {
            _connectionString = configuration.GetConnectionString("MLMConnectionString") 
                ?? throw new InvalidOperationException("Connection string 'MLMConnectionString' not found.");
            _configuration = configuration;
            _logger = logger;
            _smsService = smsService;
        }

        /// <summary>
        /// ลงทะเบียนสมาชิกแบบง่าย (Easy Registration)
        /// </summary>
        public async Task<RegistrationResponse> EasyRegisterAsync(EasyRegistrationRequest request, string? currentMemberCode = null)
        {
            try
            {
                string newMemberCode = string.Empty;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_SyncMemberToM06", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // 📝 แผนก Map ข้อมูล
                        command.Parameters.AddWithValue("@CountryCode", (object)NormalizeString(request.Country) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessCountryCode", (object)NormalizeString(request.CountryBusiness) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Sponsercode", (object)NormalizeString(request.ReferrerCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Uplinecode", DBNull.Value);
                        command.Parameters.AddWithValue("@Side", (object)MapSide(request.Position) ?? DBNull.Value);

                        // 💡 ใช้วันที่ปัจจุบันถ้าไม่ได้ส่งมา
                        DateTime registerDate = ParseCustomDate(NormalizeString(request.RegistrationDate)) ?? DateTime.Now;
                        command.Parameters.AddWithValue("@Registerdate", registerDate);
                        DateTime? birthDate = ParseCustomDate(NormalizeString(request.BirthDate));
                        command.Parameters.AddWithValue("@birthDate", (object)birthDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NameTitle", (object)NormalizeString(request.Title) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IdcardName", (object)NormalizeString(request.IdCardName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessName", (object)NormalizeString(request.BusinessName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDCard", (object)NormalizeString(request.DocumentNumber) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@MobilePhone", (object)NormalizeString(request.Mobile) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Homephone", (object)NormalizeString(request.HomePhone) ?? DBNull.Value);
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
                        // Build memberpic JSON (if client sent base64 images) -> save files to Images/Memberpicture and store paths
                        string? memberPicJson = null;
                        if (request.Memberpic != null && request.Memberpic.Count > 0)
                        {
                            var saved = SaveMemberPicsToDisk(request.Memberpic);
                            if (saved != null && saved.Count > 0)
                            {
                                memberPicJson = JsonSerializer.Serialize(saved);
                            }
                        }
                        command.Parameters.AddWithValue("@memberpic", (object?)memberPicJson ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Createby", (object)NormalizeString(currentMemberCode) ?? DBNull.Value);

                        // 🚀 ยิงคำสั่งไปที่ Database และรอรับค่าที่ SELECT กลับมา
                        var result = await command.ExecuteScalarAsync();

                        // 🔄 แปลงผลลัพธ์ที่ได้เป็น string
                        newMemberCode = result?.ToString() ?? string.Empty;

                        // 🛡️ เช็คเงื่อนไข Error จาก Store Procedure
                        if (newMemberCode == "ERROR_DUPLICATE")
                        {
                            _logger.LogWarning("Registration failed: Duplicate member code for {DocumentNumber}", request.DocumentNumber);

                            return new RegistrationResponse
                            {
                                Success = false,
                                Message = "มีรหัสสมาชิกหรือข้อมูลนี้อยู่ในระบบแล้ว",
                                MemberName = request.IdCardName
                            };
                        }

                        // 🛡️ กันเหนียว กรณี Store Procedure ทำงานจบแต่ไม่ยอม Return อะไรกลับมา
                        if (string.IsNullOrWhiteSpace(newMemberCode))
                        {
                            throw new Exception("ลงทะเบียนในฐานข้อมูลสำเร็จ แต่ไม่ได้รับรหัสสมาชิก (MemberCode) กลับมาจากระบบ");
                        }
                    }
                }

                // 🎉 บันทึก Log เมื่อสำเร็จ
                _logger.LogInformation("Easy registration successful for {DocumentNumber}. Generated MemberCode: {MemberCode}",
                    request.DocumentNumber, newMemberCode);

                // 📦 ส่งค่ากลับไปให้ Controller -> Client
                return new RegistrationResponse
                {
                    Success = true,
                    Message = "ลงทะเบียนสำเร็จ",
                    MemberName = request.IdCardName,
                    MemberCode = newMemberCode,       // แนบรหัสสมาชิกใหม่กลับไปด้วย
                    RegistrationDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                // 🚨 จัดการกรณีเกิด Error ร้ายแรง (เช่น เน็ตหลุด, Database ล่ม)
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
                        command.Parameters.AddWithValue("@BusinessCountryCode", (object)NormalizeString(request.CountryBusiness) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Sponsercode", (object)NormalizeString(request.ReferrerCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Uplinecode", (object)NormalizeString(request.UplineCode) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Side", (object)MapSide(request.UplineSide ?? request.Position) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Registerdate", DateTime.Now.ToString("yyyy-MM-dd"));
                        DateTime? birthDate = ParseCustomDate(NormalizeString(request.BirthDate));
                        command.Parameters.AddWithValue("@birthDate", (object)birthDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NameTitle", (object)NormalizeString(request.Title) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IdcardName", (object)NormalizeString(request.IdCardName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BusinessName", (object)NormalizeString(request.BusinessName) ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDCard", (object)NormalizeString(request.DocumentNumber) ?? DBNull.Value);
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

                _logger.LogInformation("Full registration successful for {DocumentNumber}", request.DocumentNumber);

                return new RegistrationResponse
                {
                    Success = true,
                    Message = "ลงทะเบียนสำเร็จ",
                    MemberName = request.IdCardName,
                    RegistrationDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during full registration for {DocumentNumber}", request.DocumentNumber);
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
                    DocumentNumber = request.DocumentNumber,
                    ReferrerCode = request.ReferrerCode,
                    Title = request.Title,
                    IdCardName = request.IdCardName,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    LineId = request.LineId,
                    AddressIdCard = request.AddressIdCard,
                    Postcode = request.Postcode,
                    ProvinceCode = request.ProvinceCode,
                    DistrictCode = request.DistrictCode,
                    SubdistrictCode = request.SubdistrictCode,
                    VerificationMethod = request.VerificationMethod,
                    CountryBusiness = request.CountryBusiness,
                    Position = request.Position,
                    RegistrationDate = request.RegistrationDate,
                    BirthDate = request.BirthDate,
                    BusinessName = request.BusinessName,
                    HomePhone = request.HomePhone
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

        private DateTime? ParseCustomDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            // ระบบหน้าบ้านส่งมาเป็น dd-MM-yyyy (เช่น 17-02-2026)
            if (DateTime.TryParseExact(dateStr, "dd-MM-yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            // Fallback เผื่อส่งมาฟอร์แมตอื่น
            if (DateTime.TryParse(dateStr, out result)) return result;

            return null;
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
            // If client provided Memberpic (base64 or already paths), save base64 -> files and return saved paths
            if (request.Memberpic != null && request.Memberpic.Count > 0)
            {
                var saved = SaveMemberPicsToDisk(request.Memberpic);
                if (saved != null && saved.Count > 0)
                {
                    return JsonSerializer.Serialize(saved);
                }

                // If SaveMemberPicsToDisk returned empty, fallthrough to other sources
            }

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

        // Save base64 images to disk under TheStarRichyProject/wwwroot/Images/Memberpicture when possible
        // Returns list of web-relative paths like "/Images/Memberpicture/filename.jpg"
        private static List<string>? SaveMemberPicsToDisk(List<string>? base64List)
        {
            if (base64List == null || base64List.Count == 0) return null; // nothing to do

            var saved = new List<string>();
            try
            {
                // Prefer saving into the web project's wwwroot so files are web-accessible.
                // Look for sibling folder named "TheStarRichyProject" by traversing upwards.
                string current = Directory.GetCurrentDirectory();
                string? rootCandidate = current;
                string? webProjectRoot = null;

                for (int i = 0; i < 6 && rootCandidate != null; i++)
                {
                    var sibling = Path.Combine(rootCandidate, "TheStarRichyProject");
                    if (Directory.Exists(sibling))
                    {
                        webProjectRoot = sibling;
                        break;
                    }
                    rootCandidate = Directory.GetParent(rootCandidate)?.FullName;
                }

                string imagesDir;
                if (!string.IsNullOrEmpty(webProjectRoot))
                {
                    imagesDir = Path.Combine(webProjectRoot, "wwwroot", "Images", "Memberpicture");
                }
                else
                {
                    // fallback to app's local Images folder
                    imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "Images", "Memberpicture");
                }

                if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

                var dataUriRegex = new Regex(@"^data:(?<mime>[^;]+);base64,(?<data>.+)$", RegexOptions.Compiled);

                foreach (var item in base64List)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;

                    // if item already looks like a path/URL keep as-is
                    if (item.StartsWith("/") || item.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        saved.Add(item);
                        continue;
                    }

                    string base64Data = item;
                    string? mime = null;
                    var m = dataUriRegex.Match(item);
                    if (m.Success)
                    {
                        mime = m.Groups["mime"].Value;
                        base64Data = m.Groups["data"].Value;
                    }

                    byte[] bytes;
                    try
                    {
                        bytes = Convert.FromBase64String(base64Data);
                    }
                    catch
                    {
                        // invalid base64 -> skip
                        continue;
                    }

                    // limit size to 5 MB per file
                    const int maxBytes = 5 * 1024 * 1024;
                    if (bytes.Length > maxBytes) continue;

                    string ext = ".jpg";
                    if (!string.IsNullOrWhiteSpace(mime))
                    {
                        if (mime.Contains("png")) ext = ".png";
                        else if (mime.Contains("jpeg") || mime.Contains("jpg")) ext = ".jpg";
                        else if (mime.Contains("gif")) ext = ".gif";
                        else ext = ".jpg"; // fallback
                    }

                    var fileName = $"member_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(imagesDir, fileName);

                    File.WriteAllBytes(filePath, bytes);

                    // If saved under web project's wwwroot, return path starting with /Images/Memberpicture
                    if (!string.IsNullOrEmpty(webProjectRoot))
                    {
                        saved.Add($"/Images/Memberpicture/{fileName}");
                    }
                    else
                    {
                        // saved under API app folder, return path relative to API (still usable for storage)
                        saved.Add($"/Images/Memberpicture/{fileName}");
                    }
                }

                return saved.Count == 0 ? null : saved;
            }
            catch
            {
                return null;
            }
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
                    //string query = @"Select DLcode, DlName from [CheckMembercode] where (Membercode = @ReferrerCode) AND (DLCode = @ReferrerCode)";
                    string query = @"SELECT TOP 1 M06_PX1, M06_X5, M06_X34 FROM M06 WHERE M06_PX1 = @ReferrerCode";
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

        // Validation methods for external registration
        public async Task<ValidationResponse> CheckBlacklistAsync(string idCardNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Query CheckBlacklist - ตรวจสอบบัญชีดำ
                    string query = "SELECT count(1) FROM CheckBlackList WHERE M06_X10 = @IDCardNumber";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "เลขที่บัตรนี้ไม่สามารถสมัครได้ในขณะนี้ หากมีข้อสงสัยกรุณาติดต่อบริษัท" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking blacklist for {IDCard}", idCardNumber);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckExpireAsync(string idCardNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Query Checkexpire - ตรวจรหัสหมดอายุหรือยัง
                    string query = "SELECT MaxexpireDate FROM CheckExpire WHERE IDCardnumber = @IDCardNumber";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && DateTime.TryParse(result.ToString(), out DateTime expireDate))
                        {
                            // Get MaxExpireDate from config (default to 365 days if not set)
                            int maxExpireDays = _configuration.GetValue<int>("Registration:MaxExpireDate", 365);
                            DateTime eligibleDate = expireDate.AddDays(maxExpireDays);
                            
                            if (DateTime.Now < eligibleDate)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = $"เลขที่บัตรนี้หมดอายุยังไม่ครบ {maxExpireDays} วัน" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expire for {IDCard}", idCardNumber);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckMemberResignAsync(string idCardNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Query CheckMemberResign - ตรวจรหัสลาออกครบวัน หรือยัง
                    string query = "SELECT Numdate FROM CheckMemberResign WHERE IDCardnumber = @IDCardNumber";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && DateTime.TryParse(result.ToString(), out DateTime resignDate))
                        {
                            // Get MaxExpireDate from config (default to 365 days if not set)
                            int maxExpireDays = _configuration.GetValue<int>("Registration:MaxExpireDate", 365);
                            DateTime eligibleDate = resignDate.AddDays(maxExpireDays);
                            
                            if (DateTime.Now < eligibleDate)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = $"เลขที่บัตรนี้ลาออกยังไม่ครบ {maxExpireDays} วัน" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking member resign for {IDCard}", idCardNumber);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckSponsorCodeAsync(string memberCode)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Query CheckSponserCode - ตรวจรหัสผู้แนะนำว่ามีปัญหาหรือไม่
                    string query = "SELECT COUNT(1) FROM CheckSponserCode WHERE MemberCode = @MemberCode";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MemberCode", memberCode);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "รหัสผู้แนะนำไม่สามารถใช้งานได้กรุณาติดต่อผู้แนะนำ" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking sponsor code for {MemberCode}", memberCode);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateIDCardAsync(string idCardNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM CheckDupIDcard WHERE IDCardnumber = @IDCard";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IDCard", idCardNumber);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "บัตรประชาชนนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate ID card for {IDCard}", idCardNumber);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateIDCardNameAsync(string idCardName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM CheckDupIDcardname WHERE IDCardName = @IDCardName";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IDCardName", idCardName);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "ชื่อตามบัตรประชาชนนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate ID card name for {Name}", idCardName);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateBusinessNameAsync(string businessName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM CheckDupBusinessname WHERE Businessname = @BusinessName";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BusinessName", businessName);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "ชื่อทางธุรกิจนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate business name for {Name}", businessName);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateTelephoneAsync(string telephone)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM CheckDupTelephoneNumber WHERE Telephonenumber = @Telephone";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Telephone", telephone);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "เบอร์โทรศัพท์นี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate telephone for {Phone}", telephone);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateBankAccountAsync(string bankCode, string accountNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Query CheckDupBankAccountNumber - ตรวจเลขที่บัญชีนี้ซ้ำในระบบหรือไม่
                    string query = "SELECT COUNT(1) FROM CheckDupBankAccountNumber WHERE Bankcode = @BankCode AND BankACCountNumber = @AccountNumber";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BankCode", bankCode);
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "บัญชีธนาคารนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate bank account for {BankCode}-{Account}", bankCode, accountNumber);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateBankAccountNameAsync(string bankCode, string accountName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Query CheckDupBankAccountName - ตรวจชื่อบัญชีนี้ซ้ำในระบบหรือไม่
                    string query = "SELECT COUNT(1) FROM CheckDupBankAccountName WHERE BankCode = @BankCode AND BankAccountName = @AccountName";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BankCode", bankCode);
                        command.Parameters.AddWithValue("@AccountName", accountName);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "ชื่อบัญชีธนาคารนี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate bank account name for {BankCode}-{Name}", bankCode, accountName);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateEmailAsync(string email)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM CheckDupEmail WHERE Email = @Email";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "Email นี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate email for {Email}", email);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckDuplicateLineIdAsync(string lineId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM CheckDupLineid WHERE lineid = @LineID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LineID", lineId);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            if (count > 0)
                            {
                                return new ValidationResponse { 
                                    Success = false, 
                                    Message = "LineID นี้มีในระบบแล้วกรุณาตรวจสอบอีกครั้ง" 
                                };
                            }
                        }
                        return new ValidationResponse { Success = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate Line ID for {LineId}", lineId);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        public async Task<ValidationResponse> CheckAgeAsync(string birthDate)
        {
            try
            {
                if (!DateTime.TryParse(birthDate, out DateTime dob))
                {
                    return new ValidationResponse { Success = false, Message = "รูปแบบวันที่เกิดไม่ถูกต้อง" };
                }

                // Calculate age
                DateTime today = DateTime.Today;
                int age = today.Year - dob.Year;
                if (dob.Date > today.AddYears(-age)) age--;

                // Get MinAge from config (default to 18 if not set)
                int minAge = _configuration.GetValue<int>("Registration:MinAge", 18);

                if (age < minAge)
                {
                    return new ValidationResponse { 
                        Success = false, 
                        Message = $"อายุผู้สมัครยังไม่ถึง {minAge} ไม่สามารถสมัครได้" 
                    };
                }

                return new ValidationResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking age for birth date {BirthDate}", birthDate);
                return new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" };
            }
        }

        // OTP methods
        public async Task<SendOTPResponse> SendOTPAsync(SendOTPRequest request)
        {
            try
            {
                // Generate 6-digit OTP
                Random random = new Random();
                string otp = random.Next(100000, 999999).ToString();
                string referenceId = Guid.NewGuid().ToString();
                DateTime expiryTime = DateTime.UtcNow.AddMinutes(5); // 5 minutes expiry

                // Send OTP via SMS
                var smsResult = await _smsService.SendOTPAsync(request.Phone, otp);

                if (smsResult.Success)
                {
                    // Store OTP in database with expiration
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        // First, clean up expired OTPs for this phone number
                        string cleanupQuery = @"DELETE FROM OTP_Sessions WHERE PhoneNumber = @PhoneNumber AND ExpiryTime < @CurrentTime";
                        using (var cleanupCommand = new SqlCommand(cleanupQuery, connection))
                        {
                            cleanupCommand.Parameters.AddWithValue("@PhoneNumber", request.Phone);
                            cleanupCommand.Parameters.AddWithValue("@CurrentTime", DateTime.UtcNow);
                            await cleanupCommand.ExecuteNonQueryAsync();
                        }

                        // Insert new OTP session
                        string insertQuery = @"INSERT INTO OTP_Sessions (ReferenceId, PhoneNumber, OTP, ExpiryTime, CreatedAt, IsUsed)
                                               VALUES (@ReferenceId, @PhoneNumber, @OTP, @ExpiryTime, @CreatedAt, 0)";
                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@ReferenceId", referenceId);
                            insertCommand.Parameters.AddWithValue("@PhoneNumber", request.Phone);
                            insertCommand.Parameters.AddWithValue("@OTP", otp);
                            insertCommand.Parameters.AddWithValue("@ExpiryTime", expiryTime);
                            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }

                    _logger.LogInformation("OTP sent and stored successfully for {Phone} with reference {ReferenceId}", request.Phone, referenceId);

                    return new SendOTPResponse {
                        Success = true,
                        Message = "ส่ง OTP สำเร็จ",
                        ReferenceId = referenceId
                    };
                }
                else
                {
                    _logger.LogError("Failed to send OTP SMS to {Phone}: {Error}", request.Phone, smsResult.Message);
                    return new SendOTPResponse {
                        Success = false,
                        Message = smsResult.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Phone}", request.Phone);
                return new SendOTPResponse { Success = false, Message = "เกิดข้อผิดพลาดในการส่ง OTP" };
            }
        }

        public async Task<VerifyOTPResponse> VerifyOTPAsync(VerifyOTPRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // ตัวแปรสำหรับเก็บค่าจาก Database
                    bool recordFound = false;
                    int id = 0;
                    string storedOtp = "";
                    DateTime expiryTime = DateTime.MinValue;
                    bool isUsed = false;
                    int attempts = 0;

                    // 1. อ่านข้อมูล (Read Phase)
                    string selectQuery = @"SELECT Id, OTP, ExpiryTime, IsUsed, Attempts
                                   FROM OTP_Sessions
                                   WHERE ReferenceId = @ReferenceId AND PhoneNumber = @PhoneNumber";

                    using (var selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@ReferenceId", request.ReferenceId);
                        selectCommand.Parameters.AddWithValue("@PhoneNumber", request.Phone);

                        using (var reader = await selectCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                recordFound = true;
                                id = reader.GetInt32(0);
                                storedOtp = reader.GetString(1);
                                expiryTime = reader.GetDateTime(2);
                                isUsed = reader.GetBoolean(3);
                                attempts = reader.GetInt32(4);
                            }
                        } // <--- Reader ถูกปิดตรงนี้ (Dispose) ทำให้ Connection ว่างพร้อมสำหรับคำสั่งถัดไป
                    }

                    // 2. ตรวจสอบ Logic (Logic Phase)
                    if (!recordFound)
                    {
                        _logger.LogWarning("OTP session not found for reference {ReferenceId}", request.ReferenceId);
                        return new VerifyOTPResponse { Success = false, Message = "ไม่พบข้อมูล OTP หรือข้อมูลไม่ถูกต้อง", IsValid = false };
                    }

                    if (isUsed)
                    {
                        return new VerifyOTPResponse { Success = false, Message = "OTP นี้ถูกใช้งานแล้ว", IsValid = false };
                    }

                    if (DateTime.UtcNow > expiryTime)
                    {
                        return new VerifyOTPResponse { Success = false, Message = "OTP หมดอายุแล้ว", IsValid = false };
                    }

                    if (attempts >= 3)
                    {
                        return new VerifyOTPResponse { Success = false, Message = "พยายามยืนยัน OTP เกินจำนวนครั้งที่กำหนด", IsValid = false };
                    }

                    // 3. อัปเดตข้อมูล (Update Phase)
                    bool isValid = storedOtp == request.OTP;

                    if (isValid)
                    {
                        // Mark OTP as used
                        string updateQuery = @"UPDATE OTP_Sessions SET IsUsed = 1, UsedAt = @UsedAt WHERE Id = @Id";
                        using (var updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@UsedAt", DateTime.UtcNow);
                            updateCommand.Parameters.AddWithValue("@Id", id);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        _logger.LogInformation("OTP verified successfully for ref {ReferenceId}", request.ReferenceId);
                        return new VerifyOTPResponse { Success = true, Message = "OTP ถูกต้อง", IsValid = true };
                    }
                    else
                    {
                        // Increment attempts
                        string incrementAttemptsQuery = @"UPDATE OTP_Sessions SET Attempts = Attempts + 1 WHERE Id = @Id";
                        using (var incrementCommand = new SqlCommand(incrementAttemptsQuery, connection))
                        {
                            incrementCommand.Parameters.AddWithValue("@Id", id);
                            await incrementCommand.ExecuteNonQueryAsync();
                        }

                        _logger.LogWarning("Invalid OTP attempt for ref {ReferenceId}", request.ReferenceId);
                        return new VerifyOTPResponse { Success = false, Message = "OTP ไม่ถูกต้อง", IsValid = false };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for reference {ReferenceId}", request.ReferenceId);
                return new VerifyOTPResponse { Success = false, Message = "เกิดข้อผิดพลาดในการตรวจสอบ OTP" };
            }
        }

        // Final registration with OTP
        public async Task<RegistrationResponse> FinalizeRegistrationAsync(FinalizeRegistrationRequest request)
        {
            try
            {
                //// TODO: Verify OTP first
                //var otpVerification = await VerifyOTPAsync(new VerifyOTPRequest {
                //    Phone = request.Mobile,
                //    OTP = request.OTP,
                //    ReferenceId = request.ReferenceId
                //});

                //if (!otpVerification.IsValid)
                //{
                //    return new RegistrationResponse {
                //        Success = false,
                //        Message = "OTP ไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง"
                //    };
                //}

                // TODO: Proceed with registration using existing ExternalRegisterAsync
                // But add file URLs if available
                var externalRequest = new ExternalRegistrationRequest {
                    Country = request.Country,
                    DocumentNumber = request.DocumentNumber,
                    ReferrerCode = request.ReferrerCode,
                    Title = request.Title,
                    IdCardName = request.IdCardName,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    LineId = request.LineId,
                    AddressIdCard = request.AddressIdCard,
                    Postcode = request.Postcode,
                    ProvinceCode = request.ProvinceCode,
                    DistrictCode = request.DistrictCode,
                    SubdistrictCode = request.SubdistrictCode,
                    VerificationMethod = request.VerificationMethod,
                    SourcePage = request.SourcePage,
                    CampaignCode = request.CampaignCode,
                    CountryBusiness = request.CountryBusiness,
                    Position = request.Position,
                    RegistrationDate = request.RegistrationDate,
                    BirthDate = request.BirthDate,
                    BusinessName = request.BusinessName,
                    HomePhone = request.HomePhone
                };

                return await ExternalRegisterAsync(externalRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in final registration for {DocumentNumber}", request.DocumentNumber);
                return new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดภายในระบบ"
                };
            }
        }
    }
}
