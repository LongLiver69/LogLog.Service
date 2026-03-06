namespace LogLog.Service
{
    public class UserDto
    {
        public string? Id { get; set; }

        public string Username { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string AvatarName { get; set; } = null!;

        public float PositionRatioX { get; set; }

        public float PositionRatioY { get; set; }

        public float ZoomLevel { get; set; }
    }
}
