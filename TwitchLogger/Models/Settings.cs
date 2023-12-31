namespace TwitchLogger.Models
{
    internal class Settings
    {
        public string TwitchUsername { get; set; } = String.Empty;
        public string OAuthToken { get; set; } = String.Empty;
        public string DatabasePath { get; set; } = String.Empty;
    }
}
