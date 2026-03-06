using AutoMapper;
using LogLog.Service.Configurations;
using LogLog.Service.Domain.Entities;
using LogLog.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using MongoDB.Driver;

namespace LogLog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly KeycloakUserService _keycloak;
        private readonly IMinioClient _minio;
        private readonly IConfiguration _config;
        private readonly MongoDbService _db;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(
            KeycloakUserService keycloak,
            IMinioClient minio,
            IConfiguration config,
            MongoDbService db,
            IMapper mapper,
            ILogger<UserController> logger)
        {
            _keycloak = keycloak;
            _minio = minio;
            _config = config;
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("avatar")]
        public async Task<IActionResult> GetAvatarByUserId()
        {
            var userId = User.FindFirst("sub")?.Value;
            var user = await _db.Users.Find(_ => _.Id == userId).FirstOrDefaultAsync();

            if (user == null || string.IsNullOrEmpty(user.AvatarName))
            {
                return Ok();
            }

            var url = await _minio.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_config["MinIO:Bucket"])
                    .WithObject(user.AvatarName)
                    .WithExpiry(60 * 2)
            );

            var avatarDto = _mapper.Map<AvatarDto>(user);
            avatarDto.AvatarUrl = url;

            return Ok(avatarDto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                bool isUpdatedSuccess = await _keycloak.UpdateUser(userId!, request);

                if (isUpdatedSuccess)
                {
                    var update = Builders<User>.Update
                        .Set(x => x.FirstName, request.FirstName)
                        .Set(x => x.LastName, request.LastName)
                        .Set(x => x.Email, request.Email)
                        .Set(x => x.UpdatedAt, DateTime.UtcNow);

                    await _db.Users.UpdateOneAsync(x => x.Id == userId, update);
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Failed to update user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, ex);
            }
        }

        [HttpPut("avatar")]
        public async Task<IActionResult> UpdateUserAvatar(UpdateAvatarRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                var user = await _db.Users.Find(_ => _.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                {
                    var newUser = new User()
                    {
                        AvatarName = request.AvatarName,
                        PositionRatioX = request.PositionRatioX,
                        PositionRatioY = request.PositionRatioY,
                        ZoomLevel = request.ZoomLevel
                    };
                    await _db.Users.InsertOneAsync(newUser);
                }
                else
                {
                    var update = Builders<User>.Update
                        .Set(x => x.AvatarName, request.AvatarName)
                        .Set(x => x.PositionRatioX, request.PositionRatioX)
                        .Set(x => x.PositionRatioY, request.PositionRatioY)
                        .Set(x => x.ZoomLevel, request.ZoomLevel)
                        .Set(x => x.UpdatedAt, DateTime.UtcNow);

                    await _db.Users.UpdateOneAsync(x => x.Id == userId, update);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar");
                return StatusCode(500, ex);
            }
        }
    }
}
