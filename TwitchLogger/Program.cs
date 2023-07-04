using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLogger.Models;
using TwitchLogger.Models.Database;

namespace TwitchLogger
{
    internal class Program
    {
        private static readonly Settings settings;

        static Program()
        {
            settings = Settings.Load("settings.json");

            using var context = new Context(settings.DatabasePath);
            context.Database.EnsureCreated();
        }

        #region Event handlers
        private static void UserJoined(object? sender, OnUserJoinedArgs e)
        {
            Console.WriteLine($"{e.Username} joined channel {e.Channel}");

            Task.Run(() =>
            {
                using var context = new Context(settings.DatabasePath);

                if (context.Membership.Any(m => m.Channel.Name == e.Channel && m.User.Name == e.Username))
                {
                    return;
                }

                var channel = context.Channels.Where(c => c.Name == e.Channel).FirstOrDefault();
                channel ??= new Channel() { Name = e.Channel };

                var user = context.Users.Where(u => u.Name == e.Username).FirstOrDefault();
                user ??= new User() { Name = e.Username };

                var membership = new Models.Database.Membership()
                {
                    Channel = channel,
                    User = user
                };

                context.Membership.Add(membership);

                context.SaveChanges();
            });
        }

        private static void MessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            Task.Run(() =>
            {
                using var context = new Context(settings.DatabasePath);

                var channel = context.Channels.Where(c => c.Name == e.ChatMessage.Channel).FirstOrDefault();
                channel ??= new Channel() { Name = e.ChatMessage.Channel };

                var user = context.Users.Where(u => u.Name == e.ChatMessage.Username).FirstOrDefault();
                user ??= new User() { Name = e.ChatMessage.Username };

                var message = new Models.Database.Message()
                {
                    Channel = channel,
                    User = user,
                    Text = e.ChatMessage.Message
                };

                context.Messages.Add(message);

                context.SaveChanges();
            });
        }
        #endregion

        private static List<string> GetChannels()
        {
            using var context = new Context(settings.DatabasePath);

            return context.Channels.Select(c => c.Name).ToList();
        }

        static void Main(string[] args)
        {
            var chat = new TwitchClient(new WebSocketClient());
            var cred = new ConnectionCredentials(settings.TwitchUsername, settings.OAuthToken);

            chat.OnUserJoined += UserJoined;
            chat.OnMessageReceived += MessageReceived;

            chat.OnConnected += (_, _) => Console.WriteLine("Connected...");

            chat.Initialize(cred);
            chat.Connect();

            foreach (var channel in GetChannels())
            {
                chat.JoinChannel(channel);
            }

            bool run = true;

            while (run)
            {
                var input = Console.ReadLine();

                if (!String.IsNullOrEmpty(input) && input.StartsWith("join"))
                {
                    chat.JoinChannel(input.Split()[1].Trim());
                }

                if (input == "stop")
                {
                    run = false;
                }
            }

            chat.Disconnect();
        }
    }
}