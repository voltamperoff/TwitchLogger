using TwitchLib.Client.Events;
using TwitchLogger.Models.Database;

namespace TwitchLogger
{
    internal class Database
    {
        public string Path { get; init; }

        public Database(string path)
        {
            Path = path;

            using var context = new Context(Path);

            context.Database.EnsureCreated();
        }

        public Task UserJoinedAsync(OnUserJoinedArgs e, CancellationToken token)
        {
            return Task.Run(() =>
            {
                using var context = new Context(Path);

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
            }, token);
        }

        public Task MessageReceivedAsync(OnMessageReceivedArgs e, CancellationToken token)
        {
            return Task.Run(() =>
            {
                using var context = new Context(Path);

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
            }, token);
        }

        #region Getters
        public Task<IEnumerable<string>> GetChannelsAsync(CancellationToken token)
        {
            return Task.Run<IEnumerable<string>>(() =>
            {
                using var context = new Context(Path);

                return context
                    .Channels
                    .Select(c => c.Name)
                    .ToList();
            }, token);
        }

        public Task<IEnumerable<string>> GetUserChannelsAsync(string username, CancellationToken token)
        {
            return Task.Run<IEnumerable<string>>(() =>
            {
                using var context = new Context(Path);

                return context
                    .Membership
                    .Where(m => m.User.Name.Contains(username))
                    .Select(m => m.Channel.Name)
                    .ToList();
            }, token);
        }

        public Task<IEnumerable<string>> GetUsersMutualChannelsAsync(string first, string second, CancellationToken token)
        {
            return Task.Run<IEnumerable<string>>(() =>
            {
                using var context = new Context(Path);

                var channelsFirst = context
                    .Membership
                    .Where(m => m.User.Name.Contains(first));

                var channelsSecond = context
                    .Membership
                    .Where(m => m.User.Name.Contains(second));

                return channelsFirst
                    .Intersect(channelsSecond)
                    .Select(m => m.Channel.Name)
                    .ToList();
            }, token);
        }
        #endregion
    }
}
