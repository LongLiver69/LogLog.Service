using LogLog.Service.Configurations;
using LogLog.Service.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace LogLog.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MongoDbService _db;

        public MessageController(MongoDbService db)
        {
            _db = db;
        }

        [HttpGet("user/{senderId}")]
        public async Task<IActionResult> GetMessagesBySender(string senderId)
        {
            var messages = await _db.Messages.Find(m => m.SenderId == senderId).ToListAsync();
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            message.CreatedAt = DateTime.UtcNow;
            await _db.Messages.InsertOneAsync(message);
            return Ok(message);
        }

        //[HttpGet("contacted-users")]
        //public async Task<IActionResult> GetAllContactedUsers()
        //{
        //    var userId = User.FindFirst("sub")?.Value;
        //    var messages = await _db.Messages.Find(m => m.SenderId == userId).ToListAsync();
        //    return Ok(messages);
        //}
    }
}
