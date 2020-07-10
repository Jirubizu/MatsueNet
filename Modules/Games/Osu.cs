using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MatsueNet.Services;
using OsuSharp;

namespace MatsueNet.Modules.Games
{
    public class Osu : MatsueModule
    {
        private readonly OsuService _osuService;

        public Osu(OsuService osuService)
        {
            _osuService = osuService;
        }

        [Command("osustats"), Summary("Get osu stats from a username"), Alias("ostats")]
        public async Task OsuStats(string username, GameMode gamemode = GameMode.Standard)
        {
            var user = await _osuService.GetUser(username, gamemode);
            var embed = new EmbedBuilder();
            embed.WithAuthor("Osu", "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d3/Osu%21Logo_%282015%29.png/800px-Osu%21Logo_%282015%29.png");
            embed.WithDescription($"Global Rank: #{user.Rank:n0} | Country Rank: #{user.CountryRank:n0}");

            embed.WithTitle($"Stats for {username} | {gamemode}");
            embed.WithUrl(user.ProfileUri.ToString());
            embed.AddField("Level", $"{user.Level:F2}", true);
            embed.AddField("Total Score", $"{user.Score:n0}", true);
            embed.AddField("Ranked Score", $"{user.RankedScore:n0}", true);
            embed.AddField("Accuracy", $"{user.Accuracy:F2}" + "%", true);
            embed.AddField("PP", $"{user.PerformancePoints:n0}", true);
            embed.AddField("Play Count", $"{user.PlayCount:n0}", true);
            embed.AddField("SS Hidden", user.CountSSH, true);
            embed.AddField("SS", user.CountSS, true);
            embed.AddField("S Hidden", user.CountSH, true);
            embed.AddField("S", user.CountS, true);
            embed.AddField("A", user.CountA, true);
            embed.AddField("Time Played", $"{user.TimePlayed.Days}d {user.TimePlayed.Hours}h {user.TimePlayed.Minutes}m", true);
            if (user.JoinDate != null) embed.AddField("Join Date", user.JoinDate.Value.ToString("f"));

            embed.WithColor(Color.Magenta);

            await SendEmbedAsync(embed.Build());
        }
    }
}