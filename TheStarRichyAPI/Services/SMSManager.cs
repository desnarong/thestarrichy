using System;
using System.Text;
using System.Net;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace TheStarRichyApi.Services
{
    /// <summary>
    /// SMS Manager สำหรับส่ง SMS ผ่าน ThaiBulkSMS API
    /// </summary>
    public class SMSManager
    {
        private readonly IConfiguration _configuration;
        private string strErrorMessage;
        private string strSmsUser;
        private string strSmsPass;
        private string strURIString;

        #region Property...

        public string ErrorMessage
        {
            get { return this.strErrorMessage; }
        }

        public string URI
        {
            get { return this.strURIString; }
            set { this.strURIString = value; }
        }

        #endregion

        #region Constructor...

        public SMSManager(IConfiguration configuration)
        {
            _configuration = configuration;
            this.strURIString = "http://www.thaibulksms.com/sms_api.php";
        }

        #endregion

        #region SendMessageExt...

        public string SendMessageExt(string strMessage, string strMobileNumber)
        {
            string strResult;
            string strErrcode;
            string strPostData;

            string strSmsUser = "";
            string strSmsPass = "";
            string strSMTPServer = "";
            string strSMSSENDER = "";
            string strSMSapiSend = "";
            string strSMWelcome = "";
            string strSMSShortURL = "";
            string strSMSWelcome = "";
            string strSMSWelcomeEng = "";
            string strSendSMS = "";
            string strType = "";

            // Get SMS config from database
            string smsConnectionString = _configuration.GetConnectionString("MLMConnectionString")
                ?? throw new InvalidOperationException("MLM connection string 'MLMConnectionString' not found.");

            using (SqlConnection iConnect = new SqlConnection(smsConnectionString))
            {
                SqlCommand iCommand1 = new SqlCommand();
                iConnect.Open();
                iCommand1.Connection = iConnect;
                iCommand1.CommandType = System.Data.CommandType.Text;

                // Check Complete
                iCommand1.CommandText = "Select * from S02";
                System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(iCommand1);
                System.Data.DataSet ds = new System.Data.DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 1)
                {
                    strSmsUser = ds.Tables[0].Rows[0]["smsapikey"].ToString();
                    strSmsPass = ds.Tables[0].Rows[0]["smsapisecret"].ToString();
                    strSMTPServer = ds.Tables[0].Rows[0]["smsSMTPmail"].ToString();
                    strSMSSENDER = ds.Tables[0].Rows[0]["smssendername"].ToString();
                    strSMSapiSend = ds.Tables[0].Rows[0]["smsapiSend"].ToString();
                    strSMSShortURL = ds.Tables[0].Rows[0]["smsShortURL"].ToString();
                    strSMSWelcome = ds.Tables[0].Rows[0]["SMSWelcome"].ToString();
                    strSMSWelcomeEng = ds.Tables[0].Rows[0]["SMSWelcomeEng"].ToString();
                    strSendSMS = ds.Tables[0].Rows[0]["S02_X132"].ToString();   // 0-เปิดใช้งานการส่ง  1-ปิด
                }
                else
                {
                    strSmsUser = "";
                    strSmsPass = "";
                    strSMTPServer = "";
                    strSMSSENDER = "";
                    strSMSapiSend = "";
                    strSMSShortURL = "";
                    strSMSWelcome = "";
                    strSMSWelcomeEng = "";
                    strSendSMS = "";
                }
                iConnect.Close();
            }

            // Check if SMS sending is enabled
            if (strSendSMS == "1")
            {
                this.strErrorMessage = "การส่ง SMS ถูกปิดใช้งาน";
                return "SMS_DISABLED";
            }

            strPostData = "";
            strResult = "";
            strType = "corporate";

            strPostData = "&msisdn=" + strMobileNumber
                + "&message=" + strMessage + " " + strSMSShortURL
                + "&sender=" + strSMSSENDER
                + "&force=" + strType
                + "&shorten_url=" + "true";

            strResult = WebRequest4(strPostData, strSMSapiSend, strSmsUser, strSmsPass);

            strErrcode = "";

            Match match = Regex.Match(strResult, "\"code\"\\s*:\\s*(\\d+)");
            if (match.Success)
            {
                strErrcode = Convert.ToInt32(match.Groups[1].Value).ToString();
            }

            if (strErrcode != "" && strErrcode != "0")
            {
                if (strErrcode == "113")
                {
                    this.strErrorMessage = "จำนวนข้อความยาวเกินไป อังกฤษ 160ตัวอักษร ไทย 70ตัวอักษร";
                }
                else if (strErrcode == "111")
                {
                    this.strErrorMessage = "กรุณาระบุ Sender Number เป็นตัวเลข 4-10 หลัก";
                }
                else
                {
                    this.strErrorMessage = "เกิดข้อผิดพลาดในการส่ง SMS: " + strErrcode;
                }
            }

            return strResult;
        }
        public string SendOTPExt(string strMessage, string strMobileNumber)
        {
            string strResult;
            string strErrcode;
            string strPostData;

            string strSmsUser = "";
            string strSmsPass = "";
            string strSMTPServer = "";
            string strSMSSENDER = "";
            string strSMSapiSend = "";
            string strSMWelcome = "";
            string strSMSShortURL = "";
            string strSMSWelcome = "";
            string strSMSWelcomeEng = "";
            string strSendSMS = "";
            string strType = "";

            // Get SMS config from database
            string smsConnectionString = _configuration.GetConnectionString("MLMConnectionString")
                ?? throw new InvalidOperationException("MLM connection string 'MLMConnectionString' not found.");

            using (SqlConnection iConnect = new SqlConnection(smsConnectionString))
            {
                SqlCommand iCommand1 = new SqlCommand();
                iConnect.Open();
                iCommand1.Connection = iConnect;
                iCommand1.CommandType = System.Data.CommandType.Text;

                // Check Complete
                iCommand1.CommandText = "Select * from S02";
                System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(iCommand1);
                System.Data.DataSet ds = new System.Data.DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 1)
                {
                    strSmsUser = ds.Tables[0].Rows[0]["smsapikey"].ToString();
                    strSmsPass = ds.Tables[0].Rows[0]["smsapisecret"].ToString();
                    strSMTPServer = ds.Tables[0].Rows[0]["smsSMTPmail"].ToString();
                    strSMSSENDER = ds.Tables[0].Rows[0]["smssendername"].ToString();
                    strSMSapiSend = ds.Tables[0].Rows[0]["smsapiSend"].ToString();
                    strSMSShortURL = ds.Tables[0].Rows[0]["smsShortURL"].ToString();
                    strSMSWelcome = ds.Tables[0].Rows[0]["SMSWelcome"].ToString();
                    strSMSWelcomeEng = ds.Tables[0].Rows[0]["SMSWelcomeEng"].ToString();
                    strSendSMS = ds.Tables[0].Rows[0]["S02_X132"].ToString();   // 0-เปิดใช้งานการส่ง  1-ปิด
                }
                else
                {
                    strSmsUser = "";
                    strSmsPass = "";
                    strSMTPServer = "";
                    strSMSSENDER = "";
                    strSMSapiSend = "";
                    strSMSShortURL = "";
                    strSMSWelcome = "";
                    strSMSWelcomeEng = "";
                    strSendSMS = "";
                }
                iConnect.Close();
            }

            // Check if SMS sending is enabled
            if (strSendSMS == "1")
            {
                this.strErrorMessage = "การส่ง SMS ถูกปิดใช้งาน";
                return "SMS_DISABLED";
            }

            strPostData = "";
            strResult = "";
            strType = "corporate";

            strPostData = "&msisdn=" + strMobileNumber
                + "&message=" + strMessage
                + "&sender=" + strSMSSENDER
                + "&force=" + strType
                + "&shorten_url=" + "true";

            strResult = WebRequest4(strPostData, strSMSapiSend, strSmsUser, strSmsPass);

            strErrcode = "";

            Match match = Regex.Match(strResult, "\"code\"\\s*:\\s*(\\d+)");
            if (match.Success)
            {
                strErrcode = Convert.ToInt32(match.Groups[1].Value).ToString();
            }

            if (strErrcode != "" && strErrcode != "0")
            {
                if (strErrcode == "113")
                {
                    this.strErrorMessage = "จำนวนข้อความยาวเกินไป อังกฤษ 160ตัวอักษร ไทย 70ตัวอักษร";
                }
                else if (strErrcode == "111")
                {
                    this.strErrorMessage = "กรุณาระบุ Sender Number เป็นตัวเลข 4-10 หลัก";
                }
                else
                {
                    this.strErrorMessage = "เกิดข้อผิดพลาดในการส่ง SMS: " + strErrcode;
                }
            }

            return strResult;
        }
        #endregion

        #region WebRequest...

        private string WebRequest4(string postData, string uriString, string apiKey, string secretKey)
        {
            string result = "";

            // แปลงข้อมูลเป็น Byte Array
            byte[] data = Encoding.UTF8.GetBytes(postData);

            // สร้าง WebRequest
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            // ใส่ Authorization Header (Basic Authentication)
            string authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":" + secretKey));
            request.Headers["Authorization"] = "Basic " + authInfo;

            // ส่งข้อมูลไปยัง Server
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            // อ่านผลลัพธ์ที่ได้จาก API
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    result = responseText;
                }
            }
            catch (WebException ex)
            {
                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    string errorText = reader.ReadToEnd();
                    result = errorText;
                }
            }

            return result;
        }

        #endregion
    }
}