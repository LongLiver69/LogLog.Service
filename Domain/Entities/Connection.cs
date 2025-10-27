namespace LogLog.Service.Domain.Entities
{
    public class Connection
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public required string SignalrId { get; set; }

        public DateTime? Timestamp { get; set; }
    }
}
