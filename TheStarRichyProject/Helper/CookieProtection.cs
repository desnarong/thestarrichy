using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TheStarRichyProject.Helper
{
    internal static class CookieProtection
    {
        private static IDataProtector? _dataProtector;

        private static void Initialize(IConfiguration config)
        {
            if (_dataProtector == null)
            {
                // Use the static DataProtectionProvider.Create with a key directory
                var keysFolder = Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys");
                var provider = DataProtectionProvider.Create(keysFolder);
                _dataProtector = provider.CreateProtector("SessionCookie");
            }
        }

        internal static string NewSessionKey()
        {
            // No valid cookie, new session.
            var guidBytes = new byte[16];
            RandomNumberGenerator.Fill(guidBytes);
            var sessionKey = new Guid(guidBytes).ToString();
            return sessionKey;
        }

        internal static string Protect(IDataProtector protector, string data)
        {
            if (protector == null)
            {
                throw new ArgumentNullException(nameof(protector));
            }
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }

            var userData = Encoding.UTF8.GetBytes(data);

            var protectedData = protector.Protect(userData);
            return Convert.ToBase64String(protectedData).TrimEnd('=');
        }

        internal static string Unprotect(IDataProtector protector, string protectedText, ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(protectedText))
                {
                    return string.Empty;
                }

                var protectedData = Convert.FromBase64String(Pad(protectedText));
                if (protectedData == null)
                {
                    return string.Empty;
                }

                var userData = protector.Unprotect(protectedData);
                if (userData == null)
                {
                    return string.Empty;
                }

                return Encoding.UTF8.GetString(userData);
            }
            catch (Exception ex)
            {
                // Log the exception, but do not leak other information
                //logger.ErrorUnprotectingSessionCookie(ex);
                return string.Empty;
            }
        }

        internal static JObject? Unprotect(string protectedText, IConfiguration config)
        {
            try
            {
                if (string.IsNullOrEmpty(protectedText))
                {
                    return null;
                }

                Initialize(config);
                
                if (_dataProtector == null)
                {
                    return null;
                }

                var protectedData = Convert.FromBase64String(Pad(protectedText));
                if (protectedData == null)
                {
                    return null;
                }

                var userData = _dataProtector.Unprotect(protectedData);
                if (userData == null)
                {
                    return null;
                }

                var json = Encoding.UTF8.GetString(userData);
                return JObject.Parse(json);
            }
            catch
            {
                return null;
            }
        }

        private static string Pad(string text)
        {
            var padding = 3 - ((text.Length + 3) % 4);
            if (padding == 0)
            {
                return text;
            }
            return text + new string('=', padding);
        }
    }
}
