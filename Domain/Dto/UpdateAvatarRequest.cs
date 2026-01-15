namespace LogLog.Service
{
    public class UpdateAvatarRequest
    {
        public string AvatarName { get; set; } = null!;
        public float PositionRatioX { get; set; }
        public float PositionRatioY { get; set; }
        public float ZoomLevel { get; set; }
    }
}
