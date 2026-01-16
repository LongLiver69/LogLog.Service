using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace LogLog.Service.Services
{
    public class KeycloakUserService
    {
        private readonly HttpClient _http;
        private readonly KeycloakTokenService _tokenService;
        private readonly IConfiguration _configuration;

        public KeycloakUserService(HttpClient http, KeycloakTokenService tokenService, IConfiguration configuration)
        {
            _http = http;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task UpdateUser(string userId, UpdateUserRequest request)
        {
            var token = await _tokenService.GetAdminToken();

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var payload = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var res = await _http.PutAsync(
                $"{_configuration["Keycloak:AdminApiBaseUrl"]}/users/{userId}",
                new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new Exception($"Keycloak error: {err}");
            }
        }
    }

}
