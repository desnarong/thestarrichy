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

        /// <summary>
        /// ตรวจสอบว่าเลขบัตรประชาชน/เอกสารซ้ำหรือไม่
        /// </summary>
        Task<bool> IsDocumentNumberExistsAsync(string documentNumber);
    }
}
