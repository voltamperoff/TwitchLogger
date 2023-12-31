using System.Text.Json;
using TwitchLogger.Models;

namespace TwitchLogger
{
    internal static class SettingsProvider
    {
        private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

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

                writer.Write(JsonSerializer.Serialize<Settings>(settings, options));
            }

            using var reader = new StreamReader(path);

            return JsonSerializer.Deserialize<Settings>(reader.ReadToEnd())!;
        }
    }
}
