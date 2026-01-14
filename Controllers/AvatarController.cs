using LogLog.Service.Configurations;
using LogLog.Service.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace LogLog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvatarController : ControllerBase
    {
        private readonly MongoDbService _db;
        private readonly ILogger<AvatarController> _logger;

        public AvatarController(MongoDbService db, ILogger<AvatarController> logger)
        {
            _db = db;
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
                    await _db.Avatars.UpdateOneAsync(
                        _ => _.UserId == userId,
                        Builders<Avatar>.Update
                            .Set(x => x.AvatarName, request.AvatarName)
                            .Set(x => x.PositionRatioX, request.PositionRatioX)
                            .Set(x => x.PositionRatioY, request.PositionRatioY)
                            .Set(x => x.ZoomLevel, request.ZoomLevel)
                            .Set(x => x.UpdatedAt, DateTime.UtcNow));
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByUserId()
        {
            var userId = User.FindFirst("sub")?.Value;
            var avatar = await _db.Avatars.Find(_ => _.UserId == userId).FirstOrDefaultAsync();

            if (avatar == null)
            {
                return Ok();
            }

            return Ok(avatar);
        }
    }

    public class UpdateAvatarRequest
    {
        public string AvatarName { get; set; } = null!;
        public float PositionRatioX { get; set; }
        public float PositionRatioY { get; set; }
        public float ZoomLevel { get; set; }
    }
}