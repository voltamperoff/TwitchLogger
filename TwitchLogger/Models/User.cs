namespace TwitchLogger.Models
{
    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
