namespace TwitchLogger.Models
{
    internal class Message
    {
        public int Id { get; set; }
        public Channel Channel { get; set; } = new();
        public User User { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Text { get; set; } = String.Empty;
    }
}
