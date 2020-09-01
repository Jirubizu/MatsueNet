using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using MatsueNet.Services;
using Discord.Commands;
using System.Threading.Tasks;
using Interactivity;
using Interactivity.Pagination;
using MatsueNet.Attributes.Parameter;
using MatsueNet.Extentions;

namespace MatsueNet.Modules
{
    [Summary("Admin management commands")]
    public class Admin : MatsueModule
    {
        private readonly DiscordShardedClient _client;
        private readonly DatabaseService _database;
        private readonly InteractivityService _interactivity;

        public Admin(DiscordShardedClient client, DatabaseService db, InteractivityService iss)
        {
            _client = client;
            _database = db;
            _interactivity = iss;
        }
        

        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        [Command("Ban"), Summary("Ban a selected user given their mention")]
        public async Task Ban(SocketUser user, [Remainder] string reason = "")
        {
            await Ban(user.Id, reason);
        }

        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        [Command("Ban"), Summary("Ban a selected user given their user id")]
        public async Task Ban(ulong id, [Remainder] string reason = "")
        {
            try
            {
                await Context.Guild.AddBanAsync(id, 0, reason);
                var user = _client.GetUser(id);
                await SendSuccessAsync($"Banned {user.Id} for {reason}");
            }
            catch (Exception)
            {
                await SendErrorAsync("User either not found or is already banned");
            }
        }

        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        [Command("unban"), Summary("Unban a provided user by their ID")]
        public async Task UnBan(ulong userid)
        {
            try
            {
                await Context.Guild.RemoveBanAsync(userid);
                var user = _client.GetUser(userid);
                await SendSuccessAsync($"Unbanned {user.Username}");
            }
            catch (Exception)
            {
                await SendErrorAsync("This user is not banned");
            }
        }

        [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
        [Command("Purge"), Summary("Remove the provided amount of messages from the channel")]
        public async Task Purge([Range(1, 100)] int amount = 25)
        {
            if (Context.Channel is ITextChannel channel)
            {
                if (!(Context.Message.Channel is SocketGuildChannel))
                {
                    return;
                }

                var messages = await channel.GetMessagesAsync(amount).FlattenAsync();
                await channel.DeleteMessagesAsync(messages);
            }
        }

        [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
        [Command("cleanup"), Summary("Cleanup bot input and output from channel.")]
        public async Task Cleanup([Range(1, 100)] int amount = 25)
        {
            if (Context.Channel is ITextChannel channel)
            {
                if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
                {
                    return;
                }

                var current = await _database.LoadRecordsByGuildId(guildChannel.Guild.Id);

                var messages = await channel.GetMessagesAsync(amount).FlattenAsync();
                messages = messages.Where(m =>
                    (m.Author.Id == Context.Client.CurrentUser.Id || m.Content.StartsWith(current.Prefix)));
                await channel.DeleteMessagesAsync(messages);
            }
        }

        [RequireUserPermission(GuildPermission.Administrator), RequireBotPermission(GuildPermission.Administrator)]
        [Command("SetPrefix"), Summary("Set the prefix of the bot")]
        public async Task SetPrefix(string prefix)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await _database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.Prefix = prefix;
            await _database.UpdateGuild(current);

            await SendSuccessAsync($"Successfully set the prefix to {prefix}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels), RequireBotPermission(ChannelPermission.ManageChannels)]
        [Command("SetMusicChannel"), Summary("Set the channel where the bot should only listen to bot commands in")]
        public async Task SetMusicChannel(SocketTextChannel channel)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await _database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.MusicChannelId = channel.Id;
            await _database.UpdateGuild(current);

            await SendSuccessAsync($"Successfully set the music channel to {guildChannel.Name}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels), RequireBotPermission(ChannelPermission.ManageChannels)]
        [Command("SetAdminChannel"), Summary("Set the channel in which only admin commands should work")]
        public async Task SetAdminChannel(SocketTextChannel channel)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await _database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.AdminChannelId = channel.Id;
            await _database.UpdateGuild(current);

            await SendSuccessAsync($"Successfully set the admin channel to {guildChannel.Name}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels), RequireBotPermission(ChannelPermission.ManageChannels)]
        [Command("SetBotChannel"), Summary("Set the channel in which the bot should only listen to")]
        public async Task SetBotChannel(SocketTextChannel channel)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await _database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.BotChannelId = channel.Id;
            await _database.UpdateGuild(current);

            await SendSuccessAsync($"Successfully set the bot channel to {guildChannel.Name}");
        }

        [Command("whois"), Summary("Find out who has certain roles [use a ', ' as a separator]"), Alias("whoi")]
        public async Task WhoIs([Remainder] string roles)
        {
            if (roles.Split(", ").Any(role => !Context.Guild.Roles.Any(r => r.Name.ToLower().Contains(role.ToLower()))))
            {
                await SendErrorAsync("One of the roles you provided is not valid. Make sure the role exists");
                return;
            }

            IEnumerable<SocketGuildUser> users = Context.Guild.Users;

            users = roles.Split(", ").Aggregate(users,
                (current, role) => current.Where(u => u.Roles.Any(r => r.Name.ToLower().Contains(role.ToLower()))));
            var dict = new Dictionary<int, List<SocketGuildUser>>();

            for (var i = 0; i < ((double) users.Count() / 25); i++)
            {
                dict.Add(i, new List<SocketGuildUser>(users.Skip(i * 25).Take(25)));
            }

            var pages = new List<PageBuilder>();
            for (var i = 0; i < dict.Count; i++)
            {
                var desc = (dict.GetValueOrDefault(i) ?? new List<SocketGuildUser>()).Aggregate("",
                    (current, user) => current + $"{user.Mention}\n");
                pages.Add(new PageBuilder().WithTitle($"Users who have the following roles {roles}")
                    .WithDescription(desc).WithColor(Color.Teal));
            }

            var paginator = new StaticPaginatorBuilder();
            paginator.WithUsers(Context.User);
            paginator.WithPages(pages);
            paginator.WithFooter(PaginatorFooter.PageNumber);
            paginator.WithForBackEmojis();

            await _interactivity.SendPaginatorAsync(paginator.Build(), Context.Channel, TimeSpan.FromMinutes(2));
        }

        [Command("whoisnot"), Summary("Find out who does not have certain roles [use a ', ' as a separator]"),
         Alias("whon")]
        public async Task WhoIsNot([Remainder] string roles)
        {
            if (roles.Split(", ").Any(role => !Context.Guild.Roles.Any(r => r.Name.ToLower().Contains(role.ToLower()))))
            {
                await SendErrorAsync("One of the roles you provided is not valid. Make sure the role exists");
                return;
            }

            IEnumerable<SocketGuildUser> users = Context.Guild.Users;
            var dict = new Dictionary<int, List<SocketGuildUser>>();
            users = roles.Split(", ").Aggregate(users,
                (current, role) => current.Where(u => !u.Roles.Any(r => r.Name.ToLower().Contains(role.ToLower()))));

            for (var i = 0; i < ((double) users.Count() / 25); i++)
            {
                dict.Add(i, new List<SocketGuildUser>(users.Skip(i * 25).Take(25)));
            }

            var pages = new List<PageBuilder>();
            for (var i = 0; i < dict.Count; i++)
            {
                var desc = (dict.GetValueOrDefault(i) ?? new List<SocketGuildUser>()).Aggregate("",
                    (current, user) => current + $"{user.Mention}\n");
                pages.Add(new PageBuilder().WithTitle($"Users who do not have the following roles {roles}")
                    .WithDescription(desc).WithColor(Color.Teal));
            }

            var paginator = new StaticPaginatorBuilder();
            paginator.WithUsers(Context.User);
            paginator.WithPages(pages);
            paginator.WithFooter(PaginatorFooter.PageNumber);
            paginator.WithForBackEmojis();

            await _interactivity.SendPaginatorAsync(paginator.Build(), Context.Channel, TimeSpan.FromMinutes(2));
        }
    }
}