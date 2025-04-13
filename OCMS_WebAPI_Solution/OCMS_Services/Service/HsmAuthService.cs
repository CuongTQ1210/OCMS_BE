using Microsoft.Extensions.Logging;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class HsmAuthService : IHsmAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HsmAuthService> _logger;

        public HsmAuthService(HttpClient httpClient, ILogger<HsmAuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            var request = new HsmLoginRequest
            {
                Username = "hsmtest",
                Password = "hsmtest"
            };

            var response = await _httpClient.PostAsJsonAsync("https://demohsm.wgroup.vn/hsm/auth", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HSM login failed with status code {response.StatusCode}");
                throw new ApplicationException("Failed to authenticate with HSM service.");
            }

            var result = await response.Content.ReadFromJsonAsync<HsmLoginResponse>();

            if (result?.Result == null || result.Result.Status != "Success")
            {
                throw new ApplicationException("HSM login unsuccessful or malformed response.");
            }

            return result.Result.Token;
        }
    }

}
