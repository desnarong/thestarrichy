using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheStarRichyProject.Services
{
    /// <summary>
    /// Response model สำหรับ API ดึงเอกสารดาวน์โหลด
    /// </summary>
    public class DocumentDownloadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<DocumentDownloadItem> Data { get; set; }
    }

    public class DocumentDownloadItem
    {
        public int Num { get; set; }
        public string Filedescription { get; set; }
        public string Filelocationname { get; set; }
    }

    /// <summary>
    /// Interface สำหรับบริการเรียก API DocumentDownload
    /// </summary>
    public interface IDocumentDownloadApiService
    {
        /// <summary>
        /// ดึงรายการเอกสารทั้งหมด
        /// </summary>
        Task<DocumentDownloadResponse> GetAllDocumentsAsync(string token, string passkey);
    }

    /// <summary>
    /// บริการเรียก API DocumentDownload
    /// </summary>
    public class DocumentDownloadApiService : IDocumentDownloadApiService
    {
        private readonly IConfiguration _config;

        public DocumentDownloadApiService(IConfiguration config)
        {
            _config = config;
        }

        private RestClient CreateClient()
        {
            var options = new RestClientOptions(_config["Api:Url"])
            {
                ConfigureMessageHandler = handler =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                }
            };
            return new RestClient(options);
        }

        /// <summary>
        /// ดึงรายการเอกสารทั้งหมด
        /// GET: /api/DocumentDownload/all
        /// </summary>
        public async Task<DocumentDownloadResponse> GetAllDocumentsAsync(string token, string passkey)
        {
            try
            {
                var client = CreateClient();
                var request = new RestRequest("/DocumentDownload/all", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("X-Passkey", passkey);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<DocumentDownloadResponse>(response.Content);
                }

                return new DocumentDownloadResponse
                {
                    Success = false,
                    Message = $"ไม่สามารถดึงข้อมูลเอกสารได้: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new DocumentDownloadResponse
                {
                    Success = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }
    }
}
