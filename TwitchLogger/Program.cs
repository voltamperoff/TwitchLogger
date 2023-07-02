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
        }

        #region Event handlers
        private static void ChannelJoined(object? sender, OnJoinedChannelArgs e)
        {
            Task.Run(() =>
            {
                using var context = new Context(settings.DatabasePath);

                if (!context.Channels.Any(c => c.Name == e.Channel))
                {
                    var channel = new Models.Database.Channel()
                    {
                        Name = e.Channel
                    };

                    context.Channels.Add(channel);

                    context.SaveChanges();
                }
            });
        }

        private static void UserJoined(object? sender, OnUserJoinedArgs e)
        {
            Task.Run(() =>
            {
                using var context = new Context(settings.DatabasePath);

                if (!context.Users.Any(u => u.Name == e.Username))
                {
                    var user = new Models.Database.User()
                    {
                        Name = e.Username
                    };

                    context.Users.Add(user);

                    context.SaveChanges();
                }
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

        static void Main(string[] args)
        {
            using var context = new Context(settings.DatabasePath);
            context.Database.EnsureCreated();

            var chat = new TwitchClient(new WebSocketClient());
            var cred = new ConnectionCredentials(settings.TwitchUsername, settings.OAuthToken);

            chat.OnJoinedChannel += ChannelJoined;
            chat.OnUserJoined += UserJoined;
            chat.OnMessageReceived += MessageReceived;

            chat.OnLog += (s, e) => Console.WriteLine(e.Data);
            chat.OnConnected += (s, e) => Console.WriteLine("CONNECTED. Press Enter");

            chat.Initialize(cred);
            chat.Connect();

            foreach (var channel in context.Channels.ToList())
            {
                chat.JoinChannel(channel.Name);
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