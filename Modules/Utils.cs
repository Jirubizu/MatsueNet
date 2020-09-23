using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MatsueNet.Attributes.Preconditions;
using MatsueNet.Services;
using MatsueNet.Structures;

namespace MatsueNet.Modules
{
    [ChannelCheck(Channels.Bot)]
    [Summary("Utility commands")]
    public class Utils : MatsueModule
    {
        public CommandService CommandService { get; set; }
        public DatabaseService DatabaseService { get; set; }
        public HttpService HttpService { get; set; }
        public ConfigService ConfigService { get; set; }
        public PaginationService Paging { get; set; }
        public OCRService OcrService { get; set; }

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
            get { return Context.Client.Guilds.Select(x => x.Users.Count).Sum(); }
        }

        private int ChannelSize
        {
            get { return Context.Client.Guilds.Select(x => x.TextChannels.Count).Sum(); }
        }

        [Command("help"), Summary("Get a list of all available commands")]
        public async Task Help()
        {
            var guildChannel = (SocketGuildChannel) Context.Message.Channel;

            var results = await DatabaseService.LoadRecordsByGuildId(guildChannel.Guild.Id);

            var embed = new EmbedBuilder
            {
                Color = Color.Teal,
                Description =
                    $"To find out what commands a category is please do the following command {results.Prefix}help <Category>"
            };

            foreach (var module in CommandService.Modules)
            {
                if (module.Name.Contains("MatsueModule"))
                {
                    continue;
                }

                embed.AddField(module.Name, module.Summary, true);
            }

            await SendEmbedAsync(embed.Build());
        }

        [Command("help"), Summary("Get information about a specified command / category")]
        public async Task Help(string command)
        {
            var guildChannel = (SocketGuildChannel) Context.Message.Channel;

            var results = await DatabaseService.LoadRecordsByGuildId(guildChannel.Guild.Id);

            if (CommandService.Modules.FirstOrDefault(m =>
                    m.Name.ToLowerInvariant().Contains(command.ToLowerInvariant())) is {
                } module)
            {
                var embed = new EmbedBuilder
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
                var result = CommandService.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await SendErrorAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                var embed = new EmbedBuilder
                {
                    Color = Color.Teal,
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    embed.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Summary: {cmd.Summary}";
                        x.IsInline = false;
                    });
                }

                await SendEmbedAsync(embed.Build());
            }
        }

        [Command("Info"), Summary("Display information regarding the bot")]
        public async Task Info()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(new EmbedAuthorBuilder().WithName("Matsue").WithIconUrl(
                "https://cdn.discordapp.com/avatars/461998714916044840/c077db327a0ebf21deb4aa08bb93f7f1.png"));
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

        [Command("ocr"), Summary("Scan the image provided and output the text found")]
        public async Task Ocr(string arg = null, string arg2 = null)
        {
            List<EmbedBuilder> pages;
            if (TryGetAttachment(Context, out string attachmentUrl))
            {
                Console.WriteLine(attachmentUrl);
                if (!Regex.IsMatch(attachmentUrl, "(http(s?):)|([/|.|\\w|\\s])*\\.(?:jpe?g|gif|png)")) return;
                
                if (arg != null)
                {
                    pages = await OcrService.OcrTranslate(attachmentUrl, arg);
                }
                else
                {
                    await (ReplyAsync(await OcrService.Ocr(attachmentUrl)));
                    return;
                }
            }
            else if (arg != null &&  Regex.IsMatch(arg, "(http(s?):)|([/|.|\\w|\\s])*\\.(?:jpe?g|gif|png)"))
            {
                if (arg2 != null)
                {
                    pages = await OcrService.OcrTranslate(arg, arg2);
                }
                else
                {
                    string result = await OcrService.Ocr(arg);
                    await ReplyAsync(result);
                    return;
                }
            }
            else
            {
                await SendErrorAsync("Incorrect use of the command.");
                return;
            }

            PaginatedMessage paginator = new PaginatedMessage(pages, "OCR Translated", Color.Teal, Context.User, new AppearanceOptions{Timeout = TimeSpan.FromMinutes(10), Style = DisplayStyle.Minimal});
            await Paging.SendMessageAsync(Context.Channel, paginator);
        }
    }
}