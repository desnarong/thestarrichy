using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Net;

namespace TheStarRichyProject.Controllers
{
    public class StaticController : Controller
    {
        private readonly IConfiguration _config;

        public StaticController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> System()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var options = new RestClientOptions(_config["Api:Url"]!)
            {
                ConfigureMessageHandler = handler =>
                    new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true }
            };
            var passkey = _config["Api:Passkey"]!;
            var client = new RestClient(options);
            var request = new RestRequest("/Static/system", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Accept", "application/json");
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
                return Content(response.Content!, "application/json");
            return StatusCode((int)(response.StatusCode), response.Content);
        }
    }
}
