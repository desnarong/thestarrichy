using System.ComponentModel.DataAnnotations;

namespace TheStarRichyApi.Models
{
    /// <summary>
    /// DTO สำหรับลงทะเบียนแบบง่าย (Easy Registration)
    /// </summary>
    public class EasyRegistrationRequest
    {
        [Required(ErrorMessage = "กรุณาเลือกประเทศ")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกประเภทเอกสาร")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเลขที่เอกสาร")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผู้อ้างอิง")]
        public string ReferrerCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกคำนำหน้า")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        public string? LineId { get; set; }

        // Optional address fields
        public string? AddressIdCard { get; set; }
        public string? Postcode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? SubdistrictCode { get; set; }

        // OTP verification
        [Required(ErrorMessage = "กรุณาเลือกวิธีการยืนยันตัวตน")]
        public string VerificationMethod { get; set; } = "phone"; // phone or email
    }

    /// <summary>
    /// DTO สำหรับลงทะเบียนแบบเต็ม (Full Registration)
    /// </summary>
    public class FullRegistrationRequest
    {
        // General Information
        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string BusinessCountry { get; set; } = string.Empty;

        [Required]
        public string ReferrerCode { get; set; } = string.Empty;

        [Required]
        public string ReferrerSide { get; set; } = "left"; // left or right

        public string? UplineCode { get; set; }
        public string? UplineSide { get; set; } = "left";

        [Required]
        public string BusinessType { get; set; } = "businessman"; // businessman or user

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string NameOnCard { get; set; } = string.Empty;

        public string? BusinessName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string CitizenNumber { get; set; } = string.Empty;

        // Contact Information
        [Required]
        [Phone]
        public string Mobile { get; set; } = string.Empty;

        [Phone]
        public string? HomePhone { get; set; }

        public string? Fax { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? LineId { get; set; }
        public string? Facebook { get; set; }

        // Address Information
        public string? AddressIdCard { get; set; }
        public string? Postcode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? SubdistrictCode { get; set; }

        // Current Address (if different)
        public bool UseIdCardAddress { get; set; } = true;
        public string? CurrentAddress { get; set; }
        public string? CurrentPostcode { get; set; }
        public string? CurrentProvinceCode { get; set; }
        public string? CurrentDistrictCode { get; set; }
        public string? CurrentSubdistrictCode { get; set; }

        // Bank Information
        public string? BankCode { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankBranch { get; set; }

        // Document Uploads (Base64 or file paths)
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
        // Inherit all fields from EasyRegistrationRequest
        // เพิ่มฟิลด์เฉพาะสำหรับ external registration ถ้าจำเป็น
        public string? SourcePage { get; set; } // ระบุว่ามาจากหน้าไหน
        public string? CampaignCode { get; set; } // รหัสแคมเปญ (ถ้ามี)
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
}
