using System.IO;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace MatsueNet
{
    public class MatsueModule : ModuleBase<ShardedCommandContext>
    {
        protected async Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false,
            Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
        {
            return await Context.Channel.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler);
        }

        protected async Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null,
            bool isTts = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
        {
            return await Context.Channel.SendFileAsync(stream, filename, text, isTts, embed, options, isSpoiler);
        }

        protected async Task<IUserMessage> SendEmbedAsync(Embed embed)
        {
            return await Context.Channel.SendMessageAsync("", false, embed);
        }

        protected async Task<IUserMessage> SendErrorAsync(string error)
        {
            return await SendErrorAsync("Error", error);
        }

        protected async Task<IUserMessage> SendErrorAsync(string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"❌ {title} ❌")
                .WithDescription(description)
                .WithColor(Color.Teal)
                .Build();

            return await Context.Channel.SendMessageAsync("", false, embed);
        }

        protected async Task<IUserMessage> SendSuccessAsync(string success)
        {
            var embed = new EmbedBuilder()
                .WithTitle("☑ Success ☑")
                .WithDescription(success)
                .WithColor(Color.Teal)
                .Build();

            return await Context.Channel.SendMessageAsync("", false, embed);
        }

        protected async Task<IUserMessage> SendWarningAsync(string warning)
        {
            var embed = new EmbedBuilder()
                .WithTitle("❗ Warning ❗")
                .WithDescription(warning)
                .WithColor(Color.Teal)
                .Build();

            return await Context.Channel.SendMessageAsync("", false, embed);
        }
    }
}