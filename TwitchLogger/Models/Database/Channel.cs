namespace TwitchLogger.Models.Database
{
    internal class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
