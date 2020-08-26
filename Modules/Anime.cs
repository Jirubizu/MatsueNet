using System.Linq;
using System.Text;
using Discord.Commands;
using Miki.Anilist;
using System.Threading.Tasks;
using Discord;
using MatsueNet.Utils;

namespace MatsueNet.Modules
{
    [Summary("Ultimate weeb commands")]
    public class Anime : MatsueModule
    {
        private readonly AnilistClient _aniClient;

        public Anime(AnilistClient ac)
        {
            _aniClient = ac;
        }

        // Anime
        [Command("anisearch")]
        public Task AniSearch(string anime)
        {
            return AniSearch(anime, 0);
        }

        [Command("anisearch")]
        public async Task AniSearch(string anime, int page)
        {
            var foundAnime = await _aniClient.SearchMediaAsync(anime, page, true, MediaType.ANIME);

            if (!foundAnime.Items.Any())
            {
                if (page != 0 && page > foundAnime.PageInfo.TotalPages)
                {
                    await SendErrorAsync(
                        $"You have passed the max number of pages ({foundAnime.PageInfo.TotalPages}) for this search");
                }
                else
                {
                    await SendErrorAsync($"No results found for {anime}");
                }

                return;
            }
            var animes = new StringBuilder();

            foreach (var a in foundAnime.Items)
            {
                animes.AppendLine($"[{a.DefaultTitle}](https://anilist.co/anime/{a.Id})");
            }
            
            var embed = new EmbedBuilder();
            embed.WithTitle($"Found Anime for {anime}");
            embed.WithDescription(animes.ToString());
            embed.WithColor(Color.Teal);
            embed.WithFooter($"Page {foundAnime.PageInfo.CurrentPage} of {foundAnime.PageInfo.TotalPages} | Powered by anilist.co");

            await SendEmbedAsync(embed.Build());
        }

        [Command("aniget")]
        public async Task AniGet(string anime)
        {
            var foundAnime = await _aniClient.GetMediaAsync(anime, MediaFormat.MANGA, MediaFormat.NOVEL);
            if (foundAnime == null)
            {
                await SendErrorAsync("No anime found with the given search term");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle($"{foundAnime.NativeTitle} | {foundAnime.EnglishTitle}");
            embed.WithUrl($"https://anilist.co/anime/{foundAnime.Id}");
            embed.WithThumbnailUrl(foundAnime.CoverImage);
            embed.AddField("Description", foundAnime.Description.Length > 1024 ? new string(foundAnime.Description.Take(1020).ToArray()) + "..." : foundAnime.Description);
            embed.AddField("Episodes", (foundAnime.Episodes ?? 0).ToString());
            embed.AddField("Genres", string.Join(", ", foundAnime.Genres));
            embed.AddField("Score", foundAnime.Score + " / 100");
            embed.AddField("Status", foundAnime.Status);
            embed.WithColor(Color.Teal);
            embed.WithFooter("Powered by anilist.co");

            await SendEmbedAsync(embed.Build());
        }

        
        
        // Manga
        [Command("mansearch")]
        public async Task ManSearch(string manga)
        {
            await ManSearch(manga, 0).ConfigureAwait(false);
        }
        
        [Command("mansearch")]
        public async Task ManSearch(string manga, int page)
        {
            var foundManga = await _aniClient.SearchMediaAsync(manga, page, true, MediaType.MANGA);

            if (!foundManga.Items.Any())
            {
                if (page != 0 && page > foundManga.PageInfo.TotalPages)
                {
                    await SendErrorAsync(
                        $"You have passed the max number of pages ({foundManga.PageInfo.TotalPages}) for this search");
                }
                else
                {
                    await SendErrorAsync($"No results found for {manga}");
                }

                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle($"Found Manga for {manga}");

            var mangas = new StringBuilder();

            foreach (var m in foundManga.Items)
            {
                mangas.AppendLine($"[{m.DefaultTitle}](https://anilist.co/manga/{m.Id})");
            }

            embed.WithDescription(mangas.ToString());

            embed.WithColor(Color.Teal);
            embed.WithFooter(
                $"Page {foundManga.PageInfo.CurrentPage} of {foundManga.PageInfo.TotalPages} | Powered by anilist.co");

            await SendEmbedAsync(embed.Build());
        }

        [Command("manget")]
        public async Task ManGet(string manga)
        {
            var foundManga = await _aniClient.GetMediaAsync(manga, MediaFormat.MUSIC, MediaFormat.ONA, MediaFormat.ONE_SHOT, MediaFormat.OVA, MediaFormat.SPECIAL, MediaFormat.TV, MediaFormat.TV_SHORT);
            if (foundManga == null)
            {
                await SendErrorAsync("No manga found with the given search term");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle(foundManga.NativeTitle == null ? foundManga.EnglishTitle : $"{foundManga.NativeTitle} | {foundManga.EnglishTitle}");
            embed.WithUrl(foundManga.Url);
            embed.WithThumbnailUrl(foundManga.CoverImage);
            embed.AddField("Description", foundManga.Description.Length > 1024 ? new string(foundManga.Description.Take(1020).ToArray()) + "..." : foundManga.Description);
            embed.AddField("Volumes", (foundManga.Volumes ?? 0).ToString());
            embed.AddField("Chapters", (foundManga.Chapters ?? 0).ToString());
            embed.AddField("Genres", string.Join(", ", foundManga.Genres));
            embed.AddField("Score", foundManga.Score + " / 100");
            embed.AddField("Status", foundManga.Status ?? "Unknown");
            embed.WithColor(Color.Teal);
            embed.WithFooter("Powered by anilist.co");

            await SendEmbedAsync(embed.Build());
        }

        // Character
        [Command("chasearch")]
        public Task ChaSearch(string character)
        {
            return ChaSearch(character, 0);
        }

        [Command("chasearch")]
        public async Task ChaSearch(string character, int page)
        {
            var foundCharacter = await _aniClient.SearchCharactersAsync(character, page);

            if (!foundCharacter.Items.Any())
            {
                if (page != 0 && page > foundCharacter.PageInfo.TotalPages)
                {
                    await SendErrorAsync($"You have passed the max number of pages ({foundCharacter.PageInfo.TotalPages}) for this search");
                }
                else
                {
                    await SendErrorAsync($"No results found for {foundCharacter}");
                }

                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle($"Found characters for {character}");

            var chara = new StringBuilder();

            foreach (var c in foundCharacter.Items)
            {
                chara.AppendLine($"[{c.FirstName} {c.LastName}](https://anilist.co/character/{c.Id})");
            }

            embed.WithDescription(chara.ToString());

            embed.WithColor(Color.Teal);
            embed.WithFooter($"Page {foundCharacter.PageInfo.CurrentPage} of {foundCharacter.PageInfo.TotalPages} | Powered by anilist.co");

            await SendEmbedAsync(embed.Build());
        }
        
        [Command("chaget")]
        public async Task ChaGet(string character)
        {
            var foundCharacter = await _aniClient.GetCharacterAsync(character);
            if (foundCharacter == null)
            {
                await SendErrorAsync("No character found with the given search term");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle(foundCharacter.NativeName == null ? $"{foundCharacter.FirstName} {foundCharacter.LastName}" : $"{foundCharacter.FirstName} {foundCharacter.LastName} | {foundCharacter.NativeName}");
            embed.WithUrl(foundCharacter.SiteUrl);
            embed.WithThumbnailUrl(foundCharacter.LargeImageUrl);
            embed.AddField("Description", foundCharacter.Description.Length > 1024 ? new string(foundCharacter.Description.Take(1020).ToArray()) + "..." : foundCharacter.Description);
            embed.WithColor(Color.Teal);
            embed.WithFooter("Powered by anilist.co");

            await SendEmbedAsync(embed.Build());
        }
    }
}