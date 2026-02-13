using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using TheStarRichyApi.Models;

namespace TheStarRichyApi.Services
{
    /// <summary>
    /// SMS Service implementation ใช้ ThaiBulkSMS API
    /// </summary>
    public class SMSService : ISMSService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SMSService> _logger;
        private readonly string _smsConnectionString;

        public SMSService(IConfiguration configuration, ILogger<SMSService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smsConnectionString = configuration.GetConnectionString("MLMConnectionString")
                ?? throw new InvalidOperationException("MLM connection string 'MLMConnectionString' not found.");
        }

        /// <summary>
        /// ส่ง SMS ทั่วไป
        /// </summary>
        public async Task<SendSMSResponse> SendSMSAsync(string phoneNumber, string message)
        {
            try
            {
                var smsManager = new SMSManager(_configuration);
                var result = smsManager.SendMessageExt(message, phoneNumber);

                if (string.IsNullOrEmpty(result) || result == "SMS_DISABLED")
                {
                    _logger.LogError("Failed to send SMS to {Phone}: {Error}", phoneNumber, smsManager.ErrorMessage);
                    return new SendSMSResponse
                    {
                        Success = false,
                        Message = smsManager.ErrorMessage ?? "ไม่สามารถส่ง SMS ได้",
                        ErrorCode = result
                    };
                }

                _logger.LogInformation("SMS sent successfully to {Phone}", phoneNumber);
                return new SendSMSResponse
                {
                    Success = true,
                    Message = "ส่ง SMS สำเร็จ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {Phone}", phoneNumber);
                return new SendSMSResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการส่ง SMS: " + ex.Message
                };
            }
        }

        /// <summary>
        /// ส่ง OTP ผ่าน SMS
        /// </summary>
        public async Task<SendOTPResponse> SendOTPAsync(string phoneNumber, string otp)
        {
            try
            {
                string message = $"รหัส OTP สำหรับการยืนยันตัวตน: {otp}\nรหัสจะหมดอายุใน 5 นาที";

                var smsResult = await SendSMSAsync(phoneNumber, message);

                if (smsResult.Success)
                {
                    _logger.LogInformation("OTP {OTP} sent to {Phone}", otp, phoneNumber);
                    return new SendOTPResponse
                    {
                        Success = true,
                        Message = "ส่ง OTP สำเร็จ",
                        ReferenceId = Guid.NewGuid().ToString()
                    };
                }
                else
                {
                    _logger.LogError("Failed to send OTP to {Phone}: {Error}", phoneNumber, smsResult.Message);
                    return new SendOTPResponse
                    {
                        Success = false,
                        Message = smsResult.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Phone}", phoneNumber);
                return new SendOTPResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการส่ง OTP: " + ex.Message
                };
            }
        }
    }
}