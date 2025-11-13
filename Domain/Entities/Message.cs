using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LogLog.Service.Domain.Entities
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("sender_id")]
        public string SenderId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("receiver_id")]
        public string ReceiverId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("from_connection_id")]
        public string FromConnectionId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("to_connection_id")]
        public string ToConnectionId { get; set; } = string.Empty;

        [BsonElement("content")]
        public string? Content { get; set; }

        [BsonElement("status")]
        public int Status { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
