namespace LogLog.Service.Domain.Models
{
    public class Message
    {
        public int FromUserId { get; set; }

        public int ToUserId { get; set; }

        public string FromConnectionId { get; set; } = string.Empty;

        public string ToConnectionId { get; set; } = string.Empty;

        public string Msg { get; set; } = string.Empty;
    }
}
