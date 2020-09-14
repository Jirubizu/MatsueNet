using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MatsueNet.Attributes.Preconditions;
using MatsueNet.Services;

namespace MatsueNet.Modules
{
    [ChannelCheck(Channels.Bot)]
    [Summary("Personal commands")]
    public class Personal : MatsueModule
    {
        public BalanceService BalanceService { get; set; }

        [Command("Balance"), Summary("Get your total balance")]
        public async Task Balance()
        {
            if (!(Context.User is IGuildUser user))
            {
                return;
            }

            var embed = new EmbedBuilder
            {
                Color = Color.Teal,
                Author = new EmbedAuthorBuilder
                    {Name = $"{user.Username}'s Balance", IconUrl = $"{user.GetAvatarUrl()}"},
                Title = $"${await BalanceService.GetBalance(user.Id)}"
            };

            await SendEmbedAsync(embed.Build());
        }

        [Command("Balance"), Summary("Get someone else's balance")]
        public async Task Balance(IGuildUser user)
        {
            var embed = new EmbedBuilder
            {
                Color = Color.Teal,
                Author = new EmbedAuthorBuilder
                    {Name = $"{user.Username}'s Balance", IconUrl = $"{user.GetAvatarUrl()}"},
                Title = $"${await BalanceService.GetBalance(user.Id)}"
            };

            await SendEmbedAsync(embed.Build());
        }

        [Command("Pay"), Summary("Pay someone a given amount")]
        public async Task Pay(IGuildUser payTo, double amount)
        {
            if (!(Context.User is IGuildUser user))
            {
                return;
            }

            var result = await BalanceService.Pay(payTo.Id, user.Id, amount);

            var embed = new EmbedBuilder
            {
                Color = Color.Teal,
                Author = new EmbedAuthorBuilder {Name = "Matsue", IconUrl = Context.Client.CurrentUser.GetAvatarUrl()},
                Description = !result
                    ? "Unable to process payment as you do not have enough money."
                    : $"Payment went through. {payTo.Username} has now received ${amount}"
            };

            await SendEmbedAsync(embed.Build());
        }
    }
}