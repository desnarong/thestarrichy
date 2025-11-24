using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TheStarRichyProject.Helper;

namespace TheStarRichyProject.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data = null);
        Task<bool> DeleteAsync(string endpoint);
    }

    public class ApiService : IApiService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            IConfiguration config, 
            IHttpContextAccessor httpContext,
            ILoggerFactory loggerFactory)
        {
            _config = config;
            _httpContext = httpContext;
            _logger = loggerFactory.CreateLogger<ApiService>();
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation($"Calling GET {endpoint}");
                
                var client = CreateClient();
                var request = new RestRequest(endpoint, Method.Get);
                AddAuthHeaders(request);

                var response = await client.ExecuteAsync(request);
                
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }

                _logger.LogWarning($"GET {endpoint} returned {response.StatusCode}");
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling GET {endpoint}");
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data = null)
        {
            try
            {
                _logger.LogInformation($"Calling POST {endpoint}");
                
                var client = CreateClient();
                var request = new RestRequest(endpoint, Method.Post);
                AddAuthHeaders(request);
                
                if (data != null)
                {
                    request.AddJsonBody(data);
                }

                var response = await client.ExecuteAsync(request);
                
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }

                _logger.LogWarning($"POST {endpoint} returned {response.StatusCode}");
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling POST {endpoint}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation($"Calling DELETE {endpoint}");
                
                var client = CreateClient();
                var request = new RestRequest(endpoint, Method.Delete);
                AddAuthHeaders(request);

                var response = await client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling DELETE {endpoint}");
                throw;
            }
        }

        private RestClient CreateClient()
        {
            var options = new RestClientOptions(_config["Api:Url"])
            {
                ThrowOnAnyError = false,
                MaxTimeout = 30000,
                ConfigureMessageHandler = handler => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                }
            };
            return new RestClient(options);
        }

        private void AddAuthHeaders(RestRequest request)
        {
            var token = _httpContext.HttpContext?.Request.Cookies[CookieHelper.UserKey];
            var passkey = _config["Api:Passkey"];

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");
            
            if (!string.IsNullOrEmpty(passkey))
                request.AddHeader("X-Passkey", passkey);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
        }
    }
}
