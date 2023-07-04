namespace TwitchLogger.Models.Database
{
    internal class Membership
    {
        public Channel Channel { get; set; } = new();
        public User User { get; set; } = new();
    }
}
