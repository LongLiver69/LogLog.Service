using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LogLog.Service.Domain.Entities
{
    public class Connection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("signalr_id")]
        public string SignalrId { get; set; } = null!;

        [BsonElement("user_id")]
        public string UserId { get; set; } = null!;

        [BsonElement("user_fullname")]
        public string UserFullname { get; set; } = null!;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
