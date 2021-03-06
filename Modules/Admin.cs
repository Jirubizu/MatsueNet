﻿using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using MatsueNet.Services;
using Discord.Commands;
using System.Threading.Tasks;
using Interactivity;
using MatsueNet.Attributes.Parameter;
using MatsueNet.Attributes.Preconditions;
using MatsueNet.Extentions;

namespace MatsueNet.Modules
{
    [ChannelCheck(Channels.Admin, Channels.Bot)]
    [Summary("Admin management commands")]
    public class Admin : MatsueModule
    {
        public DiscordShardedClient Client { get; set; }
        public DatabaseService Database { get; set; }
        public PaginationService Interactivity { get; set; }

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
                var user = Client.GetUser(id);
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
                var user = Client.GetUser(userid);
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

                var current = await Database.LoadRecordsByGuildId(guildChannel.Guild.Id);

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

            var current = await Database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.Prefix = prefix;
            await Database.UpdateGuild(current);

            await SendSuccessAsync($"Successfully set the prefix to {prefix}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels), RequireBotPermission(ChannelPermission.ManageChannels)]
        [Command("SetMusicChannel"), Summary("Set the channel where the bot should only listen to bot commands in")]
        public async Task SetMusicChannel(SocketTextChannel channel = null)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await Database.LoadRecordsByGuildId(guildChannel.Guild.Id);

            current.MusicChannelId = channel?.Id;
            await Database.UpdateGuild(current);

            await SendSuccessAsync(channel != null
                ? $"Successfully set the music channel to {channel.Name}"
                : "Successfully un linked the bot from previous channel");
        }

        [RequireUserPermission(GuildPermission.ManageChannels), RequireBotPermission(ChannelPermission.ManageChannels)]
        [Command("SetAdminChannel"), Summary("Set the channel in which only admin commands should work")]
        public async Task SetAdminChannel(SocketTextChannel channel = null)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await Database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.AdminChannelId = channel?.Id;
            await Database.UpdateGuild(current);

            await SendSuccessAsync(channel != null
                ? $"Successfully set the admin channel to {channel.Name}"
                : "Successfully un linked the bot from previous channel");
        }

        [RequireUserPermission(GuildPermission.ManageChannels), RequireBotPermission(ChannelPermission.ManageChannels)]
        [Command("SetBotChannel"), Summary("Set the channel in which the bot should only listen to")]
        public async Task SetBotChannel(SocketTextChannel channel = null)
        {
            if (!(Context.Message.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            var current = await Database.LoadRecordsByGuildId(guildChannel.Guild.Id);
            current.BotChannelId = channel?.Id;
            await Database.UpdateGuild(current);

            await SendSuccessAsync(channel != null
                ? $"Successfully set the bot channel to {channel.Name}"
                : "Successfully un linked the bot from previous channel");
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

            var pages = new List<EmbedBuilder>();
            for (var i = 0; i < dict.Count; i++)
            {
                var desc = (dict.GetValueOrDefault(i) ?? new List<SocketGuildUser>()).Aggregate("",
                    (current, user) => current + $"{user.Mention}\n");
                pages.Add(new EmbedBuilder().WithDescription(desc).WithColor(Color.Teal));
            }

            var paginator = new PaginatedMessage(pages, $"Users who have the following roles {roles}", Color.Teal,
                Context.User);

            await Interactivity.SendMessageAsync(Context.Channel, paginator);
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

            var pages = new List<EmbedBuilder>();
            for (var i = 0; i < dict.Count; i++)
            {
                var desc = (dict.GetValueOrDefault(i) ?? new List<SocketGuildUser>()).Aggregate("",
                    (current, user) => current + $"{user.Mention}\n");
                pages.Add(new EmbedBuilder().WithTitle($"Users who do not have the following roles {roles}")
                    .WithDescription(desc).WithColor(Color.Teal));
            }

            var paginator = new PaginatedMessage(pages, "", Color.Teal, Context.User,
                new AppearanceOptions{Timeout = TimeSpan.FromSeconds(2)});

            await Interactivity.SendMessageAsync(Context.Channel, paginator);
        }
    }
}