using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    /// <summary>
    /// Interface สำหรับ SMS service
    /// </summary>
    public interface ISMSService
    {
        /// <summary>
        /// ส่ง SMS
        /// </summary>
        Task<SendSMSResponse> SendSMSAsync(string phoneNumber, string message);

        /// <summary>
        /// ส่ง OTP ผ่าน SMS
        /// </summary>
        Task<SendOTPResponse> SendOTPAsync(string phoneNumber, string otp);
    }

    /// <summary>
    /// Response สำหรับการส่ง SMS
    /// </summary>
    public class SendSMSResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}