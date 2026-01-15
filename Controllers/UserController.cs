using LogLog.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogLog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly KeycloakUserService _keycloak;

        public UsersController(KeycloakUserService keycloak)
        {
            _keycloak = keycloak;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserRequest request)
        {
            await _keycloak.UpdateUser(id, request);
            return Ok();
        }
    }
}
