using Discord;
using Interactivity.Pagination;

namespace MatsueNet.Extentions
{
    public static class PaginatorExtentions
    {
        public static PaginatorBuilder WithForBackEmojis(this PaginatorBuilder page)
        {
            page.Emotes.Clear();
            page.Emotes.Add(new Emoji("◀"), PaginatorAction.Backward);
            page.Emotes.Add(new Emoji("▶"), PaginatorAction.Forward);
            page.Emotes.Add(new Emoji("\xD83D\xDED1"), PaginatorAction.Exit);
            return page;
        }
    }
}