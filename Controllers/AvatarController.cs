using AutoMapper;
using LogLog.Service.Configurations;
using LogLog.Service.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using MongoDB.Driver;

namespace LogLog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvatarController : ControllerBase
    {
        private readonly IMinioClient _minio;
        private readonly IConfiguration _config;
        private readonly MongoDbService _db;
        private readonly IMapper _mapper;
        private readonly ILogger<AvatarController> _logger;

        public AvatarController(
            IMinioClient minio,
            IConfiguration config,
            MongoDbService db,
            IMapper mapper,
            ILogger<AvatarController> logger)
        {
            _minio = minio;
            _config = config;
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAvatarRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                var avatar = await _db.Avatars.Find(_ => _.UserId == userId).FirstOrDefaultAsync();

                if (avatar == null)
                {
                    var newAvatar = new Avatar()
                    {
                        AvatarName = request.AvatarName,
                        UserId = userId!,
                        PositionRatioX = request.PositionRatioX,
                        PositionRatioY = request.PositionRatioY,
                        ZoomLevel = request.ZoomLevel
                    };
                    await _db.Avatars.InsertOneAsync(newAvatar);
                }
                else
                {
                    avatar.AvatarName = request.AvatarName;
                    avatar.PositionRatioX = request.PositionRatioX;
                    avatar.PositionRatioY = request.PositionRatioY;
                    avatar.ZoomLevel = request.ZoomLevel;
                    avatar.UpdatedAt = DateTime.UtcNow;

                    await _db.Avatars.ReplaceOneAsync(_ => _.UserId == userId, avatar);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar");
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByUserId()
        {
            var userId = User.FindFirst("sub")?.Value;
            var avatar = await _db.Avatars.Find(_ => _.UserId == userId).FirstOrDefaultAsync();

            if (avatar == null || string.IsNullOrEmpty(avatar.AvatarName))
            {
                return Ok();
            }

            var url = await _minio.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_config["MinIO:Bucket"])
                    .WithObject(avatar.AvatarName)
                    .WithExpiry(60 * 2)
            );

            var avatarDto = _mapper.Map<AvatarDto>(avatar);
            avatarDto.AvatarUrl = url;

            return Ok(avatarDto);
        }
    }

}