using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MatsueNet.Services;

namespace MatsueNet.Modules
{
    [Summary("Personal commands")]
    public class Personal : MatsueModule
    {
        private readonly BalanceService _balanceService;

        public Personal(BalanceService balanceService)
        {
            _balanceService = balanceService;
        }
        
        [Command("Balance"), Summary("Get your total balance")]
        public async Task Balance()
        {
            if (!(Context.User is IGuildUser user)) return;
            
            var embed = new EmbedBuilder
            {
                Color = Color.Teal
            };
            
            embed.WithAuthor($"{user.Username}'s Balance", $"{user.GetAvatarUrl()}");
            embed.WithTitle($"${await _balanceService.GetBalance(user.Id)}");

            await SendEmbedAsync(embed.Build());
        }
        
        [Command("Balance"), Summary("Get someone else's balance")]
        public async Task Balance(IGuildUser user)
        {
            var embed = new EmbedBuilder
            {
                Color = Color.Teal
            };
            
            embed.WithAuthor($"{user.Username}'s Balance", $"{user.GetAvatarUrl()}");
            embed.WithTitle($"${await _balanceService.GetBalance(user.Id)}");

            await SendEmbedAsync(embed.Build());
        }

        [Command("Pay"), Summary("Pay someone a given amount")]
        public async Task Pay(IGuildUser payTo, double amount)
        {
            if (!(Context.User is IGuildUser user)) return;
            
            var result = await _balanceService.Pay(payTo.Id, user.Id, amount);

            var embed = new EmbedBuilder
            {
                Color = Color.Teal,
                Author = new EmbedAuthorBuilder{Name = "Matsue", IconUrl = Context.Client.CurrentUser.GetAvatarUrl()}
            };

            embed.WithDescription(!result
                ? "Unable to process payment as you do not have enough money."
                : $"Payment went through. {payTo.Username} has now received ${amount}");

            await SendEmbedAsync(embed.Build());
        }
    }
}