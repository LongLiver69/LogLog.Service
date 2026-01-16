using System.Text.Json;

namespace LogLog.Service.Services
{
    public class KeycloakTokenService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public KeycloakTokenService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<string> GetAdminToken()
        {
            var form = new Dictionary<string, string?>
            {
                ["client_id"] = _configuration["Keycloak:ClientId"],
                ["client_secret"] = _configuration["Keycloak:ClientSecret"],
                ["grant_type"] = "client_credentials"
            };

            var res = await _http.PostAsync(
                _configuration["Keycloak:TokenEndpoint"],
                new FormUrlEncodedContent(form));

            var json = await res.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString() ?? "";
        }
    }

}
