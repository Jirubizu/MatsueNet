using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using MatsueNet.Attributes.Preconditions;
using MatsueNet.Services;

namespace MatsueNet.Modules.Games
{
    [ChannelCheck(Channels.Bot)]
    [Summary("Minecraft related commands")]
    public class Minecraft : MatsueModule
    {
        public MinecraftService MinecraftService { get; set; }

        [Command("minecraftskin"), Summary("Get the skin of the provided username"), Alias("mcskin")]
        public async Task GetSkin(string username)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor("Minecraft", "https://pbs.twimg.com/profile_images/454943636590186497/K4op_GD2.jpeg");
            embed.WithImageUrl("https://minecraft.tools/en/skins/getskin.php?name=" + username);
            embed.WithColor(Color.Red);
            embed.WithFooter("Powered by Mojang API");

            await SendEmbedAsync(embed.Build());
        }

        [Command("minecraftuuid"), Summary("Get the UUID of the provided username"), Alias("mcuuid")]
        public async Task Uuid(string username)
        {
            var result = await MinecraftService.GetUuid(username);
            var embed = new EmbedBuilder();
            embed.WithAuthor("Minecraft", "https://pbs.twimg.com/profile_images/454943636590186497/K4op_GD2.jpeg");
            embed.WithColor(Color.Red);
            string uuidLong = result.Uuid.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
            embed.AddField(result.Name, result.Uuid + "\n" + uuidLong);
            embed.WithFooter("Powered by Mojang API");

            await SendEmbedAsync(embed.Build());
        }

        [Command("minecraftnames"), Summary("Get previous usernames from current username"), Alias("mcnames")]
        public async Task PreviousNames(string username)
        {
            var result = await MinecraftService.GetNames(username);
            if (result.Count < 2)
            {
                await SendWarningAsync($"No previous names found for {username}");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithAuthor("Minecraft", "https://pbs.twimg.com/profile_images/454943636590186497/K4op_GD2.jpeg");
            embed.WithTitle($"Previous usernames for {username}");

            foreach (var name in result)
            {
                var date = (new DateTime(1970, 1, 1)).AddMilliseconds(name.ChangedToInMs);
                embed.AddField(name.Name,
                    Math.Abs(name.ChangedToInMs) > 0 ? $"Changed on {date.ToString("D")}" : "First username");
            }

            embed.WithColor(Color.Red);
            embed.WithFooter("Powered by Mojang API");

            await SendEmbedAsync(embed.Build());
        }
    }
}