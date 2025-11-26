namespace LogLog.Service
{
    public class UserDto
    {
        public string? UserId { get; set; }

        public string? UserFullname { get; set; }

        public string SignalrId { get; set; } = string.Empty;
    }
}
