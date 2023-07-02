namespace TwitchLogger.Models
{
    internal class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
