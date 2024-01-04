using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLogger.Models;

namespace TwitchLogger
{
    internal class Program
    {
        private static readonly Settings settings = SettingsProvider.Load("settings.json");
        private static readonly CancellationTokenSource cancellationTokenSource = new();

        static async Task Main(string[] args)
        {
            var token = cancellationTokenSource.Token;

            var db = new Database(settings.DatabasePath);

            var chat = new TwitchClient(new WebSocketClient());
            var cred = new ConnectionCredentials(settings.TwitchUsername, settings.OAuthToken);

            chat.OnUserJoined += async (_, e) => await db.LogUserMembershipAsync(e, token);
            chat.OnMessageReceived += async (_, e) => await db.LogChatMessageAsync(e, token);

            chat.Initialize(cred);
            chat.Connect();

            foreach (var channel in await db.GetTrackedChannelsAsync(token))
            {
                chat.JoinChannel(channel);
            }

            while (!token.IsCancellationRequested)
            {
                var input = Console.ReadLine();

                if (!String.IsNullOrEmpty(input) && input.StartsWith("join"))
                {
                    chat.JoinChannel(input.Split()[1].Trim());
                }

                if (!String.IsNullOrEmpty(input) && input.StartsWith("member"))
                {
                    Console.WriteLine(String.Join(", ", await db.GetUserChannelsAsync(input.Split()[1].Trim(), token)));
                }

                if (input == "stop")
                {
                    cancellationTokenSource.Cancel();
                }
            }

            chat.Disconnect();
        }
    }
}