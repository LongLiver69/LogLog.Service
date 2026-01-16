using LogLog.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogLog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly KeycloakUserService _keycloak;

        public UserController(KeycloakUserService keycloak)
        {
            _keycloak = keycloak;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;
            await _keycloak.UpdateUser(userId!, request);
            return Ok();
        }
    }
}
