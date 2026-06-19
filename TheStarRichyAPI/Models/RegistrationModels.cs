using System.ComponentModel.DataAnnotations;

namespace TheStarRichyApi.Models
{
    /// <summary>
    /// DTO สำหรับลงทะเบียนแบบง่าย (Easy Registration)
    /// </summary>
    public class EasyRegistrationRequest
    {
        public string Country { get; set; } = string.Empty;

        public string DocumentNumber { get; set; } = string.Empty;

        public string ReferrerCode { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? LineId { get; set; }

        public string? UplineCode { get; set; }

        // Optional address fields
        public int tambonId { get; set; }
        public string? AddressIdCard { get; set; }
        public string? Postcode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? SubdistrictCode { get; set; }

        public string VerificationMethod { get; set; } = "phone";

        // 💡 ฟิลด์เหล่านี้มีส่งมาใน JSON ทั้งหมด (รวมไว้ในคลาสแม่ที่เดียว)
        public string? CountryBusiness { get; set; }
        public string? Position { get; set; }
        public string? RegistrationDate { get; set; }
        public string? BirthDate { get; set; }
        public string? IdCardName { get; set; }
        public string? BusinessName { get; set; }
        public string? HomePhone { get; set; }

        // Uploaded member pictures (base64 strings). Maps from JSON property `memberpic`.
        public List<string>? Memberpic { get; set; }

        public string? ipAddress { get; set; }
    }

    /// <summary>
    /// DTO สำหรับลงทะเบียนแบบเต็ม (Full Registration)
    /// </summary>

    public class FullRegistrationRequest
    {
        // ==========================================
        // 🟢 กลุ่มฟิลด์ที่ปรับชื่อให้ตรงกับ EasyRegistrationRequest
        // ==========================================
        public string Country { get; set; } = string.Empty;

        // เดิม BusinessCountry -> เปลี่ยนเป็น CountryBusiness
        public string? CountryBusiness { get; set; }

        public string ReferrerCode { get; set; } = string.Empty;

        // เดิม ReferrerSide -> เปลี่ยนเป็น Position
        public string? Position { get; set; }

        public string Title { get; set; } = string.Empty;

        // เดิม NameOnCard -> เปลี่ยนเป็น IdCardName
        public string? IdCardName { get; set; }

        public string? BusinessName { get; set; }

        public string? BirthDate { get; set; }

        // เดิม CitizenNumber -> เปลี่ยนเป็น DocumentNumber
        public string DocumentNumber { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string? HomePhone { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? LineId { get; set; }

        // Address Information (บัตรประชาชน)
        public string? AddressIdCard { get; set; }
        public string? Postcode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? SubdistrictCode { get; set; }
        public int tambonId { get; set; }

        // Uploaded member pictures
        public List<string>? Memberpic { get; set; }

        public string? ipAddress { get; set; }


        // ==========================================
        // 🔵 กลุ่มฟิลด์เพิ่มเติมที่มีเฉพาะใน FullRegistration (คงชื่อเดิมไว้)
        // ==========================================
        public string? UplineCode { get; set; }
        public string? UplineSide { get; set; } = "left";

        public string BusinessType { get; set; } = "businessman"; // businessman or user

        public string? Fax { get; set; }
        public string? Facebook { get; set; }

        // Current Address (ที่อยู่จัดส่ง / ปัจจุบัน)
        public bool UseIdCardAddress { get; set; } = true;
        public string? CurrentAddress { get; set; }
        public string? CurrentPostcode { get; set; }
        public string? CurrentProvinceCode { get; set; }
        public string? CurrentDistrictCode { get; set; }
        public string? CurrentSubdistrictCode { get; set; }
        public int currentTambonId { get; set; }

        // Bank Information
        public string? BankCode { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankBranch { get; set; }

        // Document Uploads (แยกตามประเภทไฟล์)
        public string? IdCardImageFront { get; set; }
        public string? IdCardImageBack { get; set; }
        public string? BankBookImage { get; set; }
        public string? ApplicationFormImage { get; set; }
        public string? ProfileImage { get; set; }
    }

    /// <summary>
    /// DTO สำหรับลงทะเบียนจากภายนอก (External Registration - ไม่ต้อง login)
    /// ใช้รูปแบบเดียวกับ EasyRegistration
    /// </summary>
    public class ExternalRegistrationRequest : EasyRegistrationRequest
    {
        // 💡 ลบฟิลด์ที่ซ้ำซ้อนออกทั้งหมด เพราะสืบทอดมาจาก EasyRegistrationRequest แล้ว
        // เหลือไว้เฉพาะฟิลด์ที่คลาสแม่ไม่มีจริงๆ
        public string? SourcePage { get; set; }
        public string? CampaignCode { get; set; }
    }

    /// <summary>
    /// Response model สำหรับการลงทะเบียน
    /// </summary>
    public class RegistrationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public Dictionary<string, string>? Errors { get; set; }
    }

    /// <summary>
    /// Request model สำหรับค้นหาข้อมูลผู้อ้างอิง
    /// </summary>
    public class FindReferrerRequest
    {
        [Required(ErrorMessage = "กรุณากรอกรหัสผู้อ้างอิง")]
        public string ReferrerCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model สำหรับข้อมูลผู้อ้างอิง
    /// </summary>
    public class FindReferrerResponse
    {
        public bool Success { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberName { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response model สำหรับ validation APIs
    /// </summary>
    public class ValidationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model สำหรับส่ง OTP
    /// </summary>
    public class SendOTPRequest
    {
        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกวิธีการส่ง OTP")]
        public string Method { get; set; } = "sms"; // sms or email
    }

    /// <summary>
    /// Response model สำหรับส่ง SMS
    /// </summary>
    public class SendSMSResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }

    /// <summary>
    /// Response model สำหรับส่ง OTP
    /// </summary>
    public class SendOTPResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ReferenceId { get; set; } // สำหรับ track OTP session
    }

    /// <summary>
    /// Request model สำหรับ verify OTP
    /// </summary>
    public class VerifyOTPRequest
    {
        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอก OTP")]
        public string OTP { get; set; } = string.Empty;

        public string? ReferenceId { get; set; } // สำหรับ track OTP session
    }

    /// <summary>
    /// Response model สำหรับ verify OTP
    /// </summary>
    public class VerifyOTPResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// Request model สำหรับ final registration (external with OTP)
    /// </summary>
    public class FinalizeRegistrationRequest : ExternalRegistrationRequest
    {
        public string OTP { get; set; } = string.Empty;

        public string? ReferenceId { get; set; } // มีใน JSON: "a5ea286d-1f3e-4fe4-81b2-5c1dc1df164d"

        // TODO: Add file URLs when upload system is ready
        // public string? IdCardImageUrl { get; set; }
        // public string? ProfileImageUrl { get; set; }
    }
}
