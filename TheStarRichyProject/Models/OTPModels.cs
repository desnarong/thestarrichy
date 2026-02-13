using System.ComponentModel.DataAnnotations;

namespace TheStarRichyProject.Models
{
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
}