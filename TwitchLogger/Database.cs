﻿using TwitchLib.Client.Events;
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

        public Task LogUserMembershipAsync(OnUserJoinedArgs e, CancellationToken token)
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

                var membership = new Membership()
                {
                    Channel = channel,
                    User = user
                };

                context.Membership.Add(membership);

                context.SaveChanges();
            }, token);
        }

        public Task LogChatMessageAsync(OnMessageReceivedArgs e, CancellationToken token)
        {
            return Task.Run(() =>
            {
                using var context = new Context(Path);

                var channel = context.Channels.Where(c => c.Name == e.ChatMessage.Channel).FirstOrDefault();
                channel ??= new Channel() { Name = e.ChatMessage.Channel };

                var user = context.Users.Where(u => u.Name == e.ChatMessage.Username).FirstOrDefault();
                user ??= new User() { Name = e.ChatMessage.Username };

                var message = new Message()
                {
                    Channel = channel,
                    User = user,
                    Text = e.ChatMessage.Message
                };

                context.Messages.Add(message);

                context.SaveChanges();
            }, token);
        }

        #region Channels
        public Task TrackChannelAsync(string name, CancellationToken token)
        {
            return Task.Run(() =>
            {
                using var context = new Context(Path);

                var channel = context.Channels.Where(c => c.Name == name).FirstOrDefault();

                if (channel == null)
                {
                    context.Channels.Add(new() { Name = name });
                }

                context.Channels.Where(c => c.Name == name).FirstOrDefault()!.Track = true;

                context.SaveChanges();
            }, token);
        }

        public Task UntrackChannelAsync(string name, CancellationToken token)
        {
            return Task.Run(() =>
            {
                using var context = new Context(Path);

                var channel = context.Channels.Where(c => c.Name == name).FirstOrDefault();

                if (channel != null)
                {
                    channel.Track = false;
                }

                context.SaveChanges();
            }, token);
        }

        public Task<IEnumerable<string>> GetTrackedChannelsAsync(CancellationToken token)
        {
            return Task.Run<IEnumerable<string>>(() =>
            {
                using var context = new Context(Path);

                return context
                    .Channels
                    .Where(c => c.Track == true)
                    .Select(c => c.Name)
                    .ToList();
            }, token);
        }
        #endregion

        #region User membership
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
