namespace TwitchLogger.Models.Database
{
    internal class Message
    {
        public int Id { get; set; }
        public Channel Channel { get; set; } = new Channel();
        public User User { get; set; } = new User();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Text { get; set; } = string.Empty;
    }
}
