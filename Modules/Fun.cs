using System;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using MatsueNet.Structures;
using MatsueNet.Services;
using MatsueNet.Utils;
using Newtonsoft.Json.Linq;
using Color = Discord.Color;

namespace MatsueNet.Modules
{
    [Summary("Fun and miscellaneous commands")]
    public class Fun : MatsueModule
    {
        private readonly HttpService _http;
        private readonly RandomService _random;
        private readonly TronaldDumpService _tronald;

        public Fun(HttpService h, RandomService r, TronaldDumpService t)
        {
            _http = h;
            _random = r;
            _tronald = t;
        }

        [Command("Avatar")]
        [Summary("Get the avatar of the provided user")]
        public async Task Avatar(SocketUser user)
        {
            await SendFileAsync(StreamUtils.GetStreamFromUrl(user.GetAvatarUrl(ImageFormat.Auto, 256)), "avatar.png");
        }

        [Command("random"), Summary("Get a random image from imgur sub.")]
        public async Task Random(string sub)
        {
            var json = await _http.GetJsonAsync<ImgurJson>($"https://imgur.com/r/{Uri.EscapeDataString(sub)}/new.json");
            if (json.Success)
            {
                var image = _random.Pick(json.Data);
                if (image.Nsfw && Context.Channel is ITextChannel channel)
                {
                    if (!channel.IsNsfw)
                    {
                        await SendErrorAsync("NotNSFW", "This tag or image is marked NSFW, please use this tag in a NSFW channel.");
                        return;
                    }
                }
                await ReplyAsync("", false, new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = image.Title,
                        IconUrl = "https://s.imgur.com/images/favicon-32x32.png"
                    },
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = image.Author
                    },
                    Color = new Color(27, 183, 110),
                    ImageUrl = "https://i.imgur.com/" + image.Hash + image.Ext,
                    Timestamp = image.Timestamp
                }.Build());
            }
        }

        [Command("tronald"), Summary("Get a random quote from the dumb donald himself")]
        public async Task Tronald()
        {
            var random = await _tronald.GetRandom();

            var embed = new EmbedBuilder();
            embed.WithAuthor("Donald Dumb", "https://i.pinimg.com/originals/e9/dd/4c/e9dd4c4e1c4eb9ef2d2a29b04fcb382f.jpg");
            embed.WithTitle($"Contains the following tag : {random.Tags[0]}");
            embed.WithColor(Color.Teal);

            embed.WithDescription(random.Value);
            embed.WithFooter(random.Embed.Source[0].Url);
            await SendEmbedAsync(embed.Build());
        }

        [Command("tronald"), Summary("Provide a tag or write tags to see all available tags")]
        public async Task Tronald(string tag)
        {
            if (tag == "tags")
            {
                var tags = await _tronald.GetTags();
                var embed = new EmbedBuilder();
                embed.WithAuthor("Donald Dumb", "https://i.pinimg.com/originals/e9/dd/4c/e9dd4c4e1c4eb9ef2d2a29b04fcb382f.jpg");
                embed.WithColor(Color.Teal);
                embed.WithTitle("Available search tags");
                var str = new StringBuilder();
                foreach (var t in tags.Embed.Tags)
                {
                    str.AppendLine(t.Value);
                }

                embed.WithDescription(str.ToString());
                await SendEmbedAsync(embed.Build());
            }
        }

        [Command("Translate"), Summary("Translate using Google translate"), Alias("gt")]
        public async Task Translate(string from, string to, [Remainder] string text)
        {
            from = Uri.EscapeDataString(from);
            to = Uri.EscapeDataString(to);

            var etext = Uri.EscapeDataString(text);
            var json = await _http.GetJArrayAsync($"https://translate.google.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={etext}");

            // ReSharper disable once PossibleNullReferenceException
            if (json.HasValues && json[0].HasValues && json[0][0].HasValues)
            {
                var embed = new EmbedBuilder();
                embed.WithColor(Color.Teal);
                embed.WithTitle("Your Translation");
                embed.AddField("Original text", text);
                embed.AddField("Translated text", (json[0][0][0] ?? "").Value<string>());
                embed.WithFooter("Powered by Google");
                await SendEmbedAsync(embed.Build());
            }
        }
    }
}