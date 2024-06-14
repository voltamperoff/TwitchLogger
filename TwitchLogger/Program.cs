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

                try
                {
                    if (!String.IsNullOrEmpty(input) && input.StartsWith("join"))
                    {
                        var name = input.Split()[1].Trim();

                        if (String.IsNullOrWhiteSpace(name))
                        {
                            Console.WriteLine("Channel name must be specified");
                            continue;
                        }

                        await db.TrackChannelAsync(name, token);
                        chat.JoinChannel(name);
                    }

                    if (!String.IsNullOrEmpty(input) && input.StartsWith("leave"))
                    {
                        var name = input.Split()[1].Trim();

                        if (String.IsNullOrWhiteSpace(name))
                        {
                            Console.WriteLine("Channel name must be specified");
                            continue;
                        }

                        await db.UntrackChannelAsync(name, token);
                        chat.LeaveChannel(name);
                    }

                    if (!String.IsNullOrEmpty(input) && input.StartsWith("member"))
                    {
                        var name = input.Split()[1].Trim();

                        if (String.IsNullOrWhiteSpace(name))
                        {
                            Console.WriteLine("User name must be specified");
                            continue;
                        }

                        Console.WriteLine(String.Join(", ", await db.GetUserChannelsAsync(name, token)));
                    }

                    if (input == "stop")
                    {
                        cancellationTokenSource.Cancel();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            chat.Disconnect();
        }
    }
}