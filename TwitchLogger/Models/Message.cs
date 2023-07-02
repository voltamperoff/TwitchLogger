namespace TwitchLogger.Models
{
    internal class Message
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Text { get; set; } = String.Empty;
    }
}
