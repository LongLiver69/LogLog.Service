using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LogLog.Service.Domain.Entities
{
    public class User
    {
        [BsonId]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = null!;

        [BsonElement("first_name")]
        public string FirstName { get; set; } = null!;

        [BsonElement("last_name")]
        public string LastName { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("avatar_name")]
        public string AvatarName { get; set; } = null!;

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
