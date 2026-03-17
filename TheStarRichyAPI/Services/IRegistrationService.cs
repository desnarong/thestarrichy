using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    /// <summary>
    /// Service interface สำหรับการลงทะเบียนสมาชิก
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// ลงทะเบียนสมาชิกแบบง่าย (Easy Registration)
        /// </summary>
        Task<RegistrationResponse> EasyRegisterAsync(EasyRegistrationRequest request, string? currentMemberCode = null);

        /// <summary>
        /// ลงทะเบียนสมาชิกแบบเต็ม (Full Registration)
        /// </summary>
        Task<RegistrationResponse> FullRegisterAsync(FullRegistrationRequest request, string? currentMemberCode = null);

        /// <summary>
        /// ลงทะเบียนจากภายนอก (External Registration - ไม่ต้อง login)
        /// </summary>
        Task<RegistrationResponse> ExternalRegisterAsync(ExternalRegistrationRequest request);

        /// <summary>
        /// ค้นหาข้อมูลผู้อ้างอิง
        /// </summary>
        Task<FindReferrerResponse> FindReferrerAsync(string referrerCode);

        // Validation methods for external registration
        Task<ValidationResponse> CheckBlacklistAsync(string idCardNumber);
        Task<ValidationResponse> CheckExpireAsync(string idCardNumber);
        Task<ValidationResponse> CheckMemberResignAsync(string idCardNumber);
        Task<ValidationResponse> CheckSponsorCodeAsync(string memberCode);
        Task<ValidationResponse> CheckDuplicateIDCardAsync(string idCardNumber);
        Task<ValidationResponse> CheckDuplicateIDCardNameAsync(string idCardName);
        Task<ValidationResponse> CheckDuplicateBusinessNameAsync(string businessName);
        Task<ValidationResponse> CheckDuplicateTelephoneAsync(string telephone);
        Task<ValidationResponse> CheckDuplicateBankAccountAsync(string bankCode, string accountNumber);
        Task<ValidationResponse> CheckDuplicateBankAccountNameAsync(string bankCode, string accountName);
        Task<ValidationResponse> CheckDuplicateEmailAsync(string email);
        Task<ValidationResponse> CheckDuplicateLineIdAsync(string lineId);
        Task<ValidationResponse> CheckAgeAsync(string birthDate);

        // OTP methods
        Task<SendOTPResponse> SendOTPAsync(SendOTPRequest request);
        Task<VerifyOTPResponse> VerifyOTPAsync(VerifyOTPRequest request);

        // Final registration with OTP
        Task<RegistrationResponse> FinalizeRegistrationAsync(FinalizeRegistrationRequest request);
    }
}
