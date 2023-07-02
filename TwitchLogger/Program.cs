using TwitchLogger.Models;

namespace TwitchLogger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var settings = Settings.Load("settings.json");
            var context = new Context(settings.DatabasePath);

            context.Database.EnsureCreated();
        }
    }
}