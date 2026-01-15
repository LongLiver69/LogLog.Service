namespace LogLog.Service
{
    public class AvatarDto
    {
        public string? Id { get; set; }

        public string AvatarName { get; set; } = null!;

        public string AvatarUrl { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public float PositionRatioX { get; set; }

        public float PositionRatioY { get; set; }

        public float ZoomLevel { get; set; }
    }
}
