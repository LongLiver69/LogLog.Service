using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LogLog.Service.Domain.Entities
{
    public class Avatar
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("avatar_name")]
        public string AvatarName { get; set; } = null!;

        [BsonElement("user_id")]
        public string UserId { get; set; } = null!;

        [BsonElement("position_ratio_x")]
        public float PositionRatioX { get; set; }

        [BsonElement("position_ratio_y")]
        public float PositionRatioY { get; set; }

        [BsonElement("user_fullname")]
        public float ZoomLevel { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
