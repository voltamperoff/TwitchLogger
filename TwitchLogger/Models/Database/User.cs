namespace TwitchLogger.Models.Database
{
    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    }
}
