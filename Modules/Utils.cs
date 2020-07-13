using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Linq;

using System.Text;
using MatsueNet.Services;

namespace MatsueNet.Modules
{
    [Summary("Utility commands")]
    public class Utils : MatsueModule
    {
        private readonly CommandService _commandService;
        private readonly DatabaseService _databaseService;

        private static string Uptime
        {
            get
            {
                var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
                return new StringBuilder()
                    .Append((time.Days != 0 ? time.Days : 00) + "d:")
                    .Append((time.Hours != 0 ? time.Hours : 00) + "h:")
                    .Append((time.Minutes != 0 ? time.Minutes : 00) + "m:")
                    .Append((time.Seconds != 0 ? time.Seconds : 00) + "s").ToString();
            }
        }

        private int UserSize
        {
            get
            {
                return Context.Client.Guilds.Select(x => x.Users.Count).Sum();
            }
        }

        private int ChannelSize
        {
            get
            {
                return Context.Client.Guilds.Select(x => x.TextChannels.Count).Sum();
            }
        }

        public Utils(CommandService commandService, DatabaseService databaseService)
        {
            _commandService = commandService;
            _databaseService = databaseService;
        }

        [Command("help"), Summary("Get a list of all available commands")]
        public async Task Help()
        {
            var guildChannel = (SocketGuildChannel)Context.Message.Channel;

            var results = await _databaseService.LoadRecordsByGuildId(guildChannel.Guild.Id);

            var embed = new EmbedBuilder()
            {
                Color = Color.Teal,
                Description = $"To find out what commands a category is please do the following command {results.Prefix}help <Category>"
            };

            foreach (var module in _commandService.Modules)
            {
                if (module.Name.Contains("MatsueModule")) continue;

                embed.AddField(module.Name, module.Summary, true);
            }

            await SendEmbedAsync(embed.Build());
        }

        [Command("help"), Summary("Get information about a specified command / category")]
        public async Task Help(string command)
        {
            var guildChannel = (SocketGuildChannel)Context.Message.Channel;

            var results = await _databaseService.LoadRecordsByGuildId(guildChannel.Guild.Id);

            if (_commandService.Modules.FirstOrDefault(m => m.Name.ToLower().Contains(command.ToLower())) is { } module)
            {
                var embed = new EmbedBuilder()
                {
                    Color = Color.Teal,
                    Description = $"Commands for {module.Name} category"
                };

                foreach (var commandInfo in module.Commands)
                {
                    embed.AddField($"{results.Prefix}{commandInfo.Name}", commandInfo.Summary);
                }

                await SendEmbedAsync(embed.Build());
            }
            else
            {
                var result = _commandService.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await SendErrorAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                var builder = new EmbedBuilder()
                {
                    Color = Color.Teal,
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Summary: {cmd.Summary}";
                        x.IsInline = false;
                    });
                }

                await SendEmbedAsync(builder.Build());
            }
        }

        [Command("Info"), Summary("Display information regarding the bot")]
        public async Task Info()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(new EmbedAuthorBuilder().WithName("Matsue").WithIconUrl("https://cdn.discordapp.com/avatars/461998714916044840/c077db327a0ebf21deb4aa08bb93f7f1.png"));
            embed.WithTitle("Bot Statistics");
            embed.AddField("Uptime", Uptime, true);
            embed.AddField("Users", UserSize, true);
            embed.AddField("Servers", Context.Client.Guilds.Count, true);
            embed.AddField("Channels", ChannelSize, true);
            embed.WithColor(Color.Teal);

            await SendEmbedAsync(embed.Build());
        }

        [Command("Invite"), Summary("Show the invite link for Matsue")]
        public async Task Invite()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor($"{Context.Client.CurrentUser.Username}", $"{Context.Client.CurrentUser.GetAvatarUrl()}");
            embed.WithTitle("Invite Me!!!!");
            embed.WithUrl($"https://discord.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot");
            embed.WithColor(Color.Teal);

            await SendEmbedAsync(embed.Build());
        }
    }
}