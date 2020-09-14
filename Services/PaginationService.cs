using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MatsueNet.Services
{
    public class PaginationService
    {
        private readonly Dictionary<ulong, PaginatedMessage> Messages;
        private readonly DiscordShardedClient client;

        public PaginationService(DiscordShardedClient client)
        {
            Messages = new Dictionary<ulong, PaginatedMessage>();
            this.client = client;
            this.client.ReactionAdded += OnReactionAdded;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(IMessageChannel channel, PaginatedMessage paginated)
        {
            IUserMessage message = await channel.SendMessageAsync("", embed: paginated.GetEmbed());

            await message.AddReactionAsync(paginated.Options.EmoteFirst);
            await message.AddReactionAsync(paginated.Options.EmoteBack);
            await message.AddReactionAsync(paginated.Options.EmoteNext);
            await message.AddReactionAsync(paginated.Options.EmoteLast);
            await message.AddReactionAsync(paginated.Options.EmoteStop);

            Messages.Add(message.Id, paginated);

            if (paginated.Options.Timeout != TimeSpan.Zero)
            {
                Task _ = Task.Delay(paginated.Options.Timeout).ContinueWith(async _t =>
                {
                    if (!Messages.ContainsKey(message.Id))
                    {
                        return;
                    }

                    Console.WriteLine(paginated.Options.TimeoutAction.ToString());

                    if (paginated.Options.TimeoutAction == StopAction.DeleteMessage)
                    {
                        await message.DeleteAsync();
                    }
                    else if (paginated.Options.TimeoutAction == StopAction.ClearReactions)
                    {
                        await message.RemoveAllReactionsAsync();
                    }

                    Messages.Remove(message.Id);
                });
            }

            return message;
        }

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();

            if (message == null)
            {
                return;
            }

            if (!reaction.User.IsSpecified)
            {
                return;
            }

            if (Messages.TryGetValue(message.Id, out PaginatedMessage page))
            {
                if (reaction.UserId == client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }

                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                if (reaction.Emote.Name == page.Options.EmoteFirst.Name)
                {
                    if (page.CurrentPage != 1)
                    {
                        page.CurrentPage = 1;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteBack.Name)
                {
                    if (page.CurrentPage != 1)
                    {
                        page.CurrentPage--;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteNext.Name)
                {
                    if (page.CurrentPage != page.Count)
                    {
                        page.CurrentPage++;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteLast.Name)
                {
                    if (page.CurrentPage != page.Count)
                    {
                        page.CurrentPage = page.Count;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteStop.Name)
                {
                    if (page.Options.EmoteStopAction == StopAction.DeleteMessage)
                    {
                        await message.DeleteAsync();
                    }
                    else if (page.Options.EmoteStopAction == StopAction.ClearReactions)
                    {
                        await message.RemoveAllReactionsAsync();
                    }

                    Messages.Remove(message.Id);
                }
            }
        }
    }

    public class PaginatedMessage
    {
        internal string Title { get; }
        internal Color EmbedColor { get; }
        internal IReadOnlyCollection<Embed> Pages { get; }
        internal IUser User { get; }
        internal AppearanceOptions Options { get; }
        internal int CurrentPage { get; set; }
        internal int Count => Pages.Count;

        public PaginatedMessage(IEnumerable<string> pages, string title = "", Color? embedColor = null, IUser user = null, AppearanceOptions options = null)
        {
            new PaginatedMessage(pages.Select(x => new EmbedBuilder().WithDescription(x)), title, embedColor, user, options);
        }
        
        public PaginatedMessage(IEnumerable<EmbedBuilder> builders, string title = "", Color? embedColor = null, IUser user = null, AppearanceOptions options = null)
        {
            List<Embed> embeds = new List<Embed>();
            int i = 1;

            foreach (var builder in builders)
            {
                builder.Title ??= title;
                builder.Color ??= embedColor ?? Color.Default;
                builder.Footer ??= new EmbedFooterBuilder().WithText($"Page {i++}/{builders.Count()}");
                embeds.Add(builder.Build());
            }

            Pages = embeds;
            Title = title;
            EmbedColor = embedColor ?? Color.Default;
            User = user;
            Options = options ?? new AppearanceOptions();
            CurrentPage = 1;
        }

        internal Embed GetEmbed()
        {
            return Pages.ElementAtOrDefault(CurrentPage - 1);
        }
    }

    public class AppearanceOptions
    {
        public const string FIRST = "⏮";
        public const string BACK = "◀";
        public const string NEXT = "▶";
        public const string LAST = "⏭";
        public const string STOP = "🛑";

        public IEmote EmoteFirst { get; set; } = new Emoji(FIRST);
        public IEmote EmoteBack { get; set; } = new Emoji(BACK);
        public IEmote EmoteNext { get; set; } = new Emoji(NEXT);
        public IEmote EmoteLast { get; set; } = new Emoji(LAST);
        public IEmote EmoteStop { get; set; } = new Emoji(STOP);
        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
        public StopAction EmoteStopAction { get; set; } = StopAction.DeleteMessage;
        public StopAction TimeoutAction { get; set; } = StopAction.DeleteMessage;
    }

    public enum StopAction
    {
        ClearReactions,
        DeleteMessage
    }
}