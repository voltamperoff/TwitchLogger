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
        private static readonly Settings settings = SettingsProvider.Load("settings.json");

        #region Event handlers
        private static Task UserJoinedAsync(object? sender, OnUserJoinedArgs e)
        {
            Console.WriteLine($"{e.Username} joined channel {e.Channel}");

            return Task.Run(() =>
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

        private static Task MessageReceivedAsync(object? sender, OnMessageReceivedArgs e)
        {
            return Task.Run(() =>
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

        private static Task ShowUserMembershipAsync(string username)
        {
            return Task.Run(() =>
            {
                using var context = new Context(settings.DatabasePath);

                var user = context.Users.Where(u => u.Name.Contains(username)).FirstOrDefault();

                if (user == null)
                {
                    Console.WriteLine($"User \"{username}\" not found");

                    return;
                }

                var channels = context
                    .Membership.Where(m => m.User == user)
                    .Join(context.Channels, m => m.Channel, c => c, (m, c) => c.Name);

                Console.WriteLine($"{user.Name}: [{String.Join(", ", channels)}]");
            });
        }

        static async Task Main(string[] args)
        {
            using var context = new Context(settings.DatabasePath);
            context.Database.EnsureCreated();

            var chat = new TwitchClient(new WebSocketClient());
            var cred = new ConnectionCredentials(settings.TwitchUsername, settings.OAuthToken);

            chat.OnUserJoined += async (s, e) => await UserJoinedAsync(s, e);
            chat.OnMessageReceived += async (s, e) => await MessageReceivedAsync(s, e);

            chat.Initialize(cred);
            chat.Connect();

            foreach (var channel in context.Channels.Select(c => c.Name))
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

                if (!String.IsNullOrEmpty(input) && input.StartsWith("member"))
                {
                    await ShowUserMembershipAsync(input.Split()[1].Trim());
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