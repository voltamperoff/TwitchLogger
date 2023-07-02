using System.Text.Json;

namespace TwitchLogger.Models
{
    internal class Settings
    {
        public string TwitchUsername { get; set; } = String.Empty;
        public string OAuthToken { get; set; } = String.Empty;
        public string DatabasePath { get; set; } = String.Empty;

        public static Settings Load(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Settings file path must be specified");
            }

            if (!File.Exists(path))
            {
                using var writer = new StreamWriter(path);

                var settings = new Settings();

                var options = new JsonSerializerOptions() { WriteIndented = true };

                writer.Write(JsonSerializer.Serialize<Settings>(settings, options));
            }

            using var reader = new StreamReader(path);

            return JsonSerializer.Deserialize<Settings>(reader.ReadToEnd())!;
        }
    }
}
