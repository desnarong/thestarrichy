using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TheStarRichyApi.Models.Kbank;

namespace TheStarRichyApi.Services
{
    public interface IKbankAuthService
    {
        Task<OAuthTokenResponse> GetAccessTokenAsync();
    }
    public class KbankAuthService : IKbankAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly KbankSettings _settings;
        private readonly ILogger<KbankAuthService> _logger;
        private string? _cachedToken;
        private DateTime _tokenExpiry;
        private readonly IMemoryCache _cache;

        public KbankAuthService(
            HttpClient httpClient,
            IOptions<KbankSettings> settings,
            ILogger<KbankAuthService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _cache = cache;
        }

        public async Task<OAuthTokenResponse> GetAccessTokenAsync()
        {
            // Return cached token if still valid
            //if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            //{
            //    return _cachedToken;
            //}

            if (_cache.TryGetValue("KbankToken", out OAuthTokenResponse token))
            {
                return token;
            }

            try
            {
                // Prepare Basic Auth header
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_settings.ConsumerKey}:{_settings.ConsumerSecret}")
                );

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/oauth/token");
                //request.Headers.Add("Content-Type", $"application/x-www-form-urlencoded");
                request.Headers.Add("Authorization", $"Basic {credentials}");

                // Add form data
                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                };
                request.Content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get Kbank access token. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    throw new Exception($"Failed to get access token: {content}");
                }

                var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(content);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new Exception("Invalid token response from Kbank");
                }

                // Cache token (expires in 1799 seconds, we'll refresh 60 seconds early)
                //_cachedToken = tokenResponse.AccessToken;
                //_tokenExpiry = DateTime.UtcNow.AddSeconds(int.Parse(tokenResponse.ExpiresIn) - 60);

                _logger.LogInformation("Successfully obtained Kbank access token");

                _cache.Set("KbankToken", tokenResponse, TimeSpan.FromSeconds(int.Parse(tokenResponse.ExpiresIn) - 60));
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Kbank access token");
                throw;
            }
        }
    }
}
