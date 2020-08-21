using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MatsueNet.Services;
using MatsueNet.Extentions;
using Victoria;
using Victoria.Enums;

namespace MatsueNet.Modules
{
    [Summary("Music commands")]
    public class Music : MatsueModule
    {
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        private readonly LavaNode _lavaNode;
        private readonly MusicService _musicService;

        public Music(LavaNode lNode, MusicService mService)
        {
            _lavaNode = lNode;
            _musicService = mService;
        }

        [Command("Join"), Summary("Join the voice channel you are currently in")]
        public async Task Join()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await SendErrorAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await SendErrorAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await SendSuccessAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Leave"), Summary("Make the bot disconnect from the current voice channel")]
        public async Task Leave()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendErrorAsync("I'm not connected to any voice channels!");
                return;
            }

            var voiceChannel = ((IVoiceState)Context.User).VoiceChannel ?? player.VoiceChannel;
            if (voiceChannel == null)
            {
                await SendErrorAsync("Not sure which voice channel to disconnect from.");
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                await SendSuccessAsync($"I've left {voiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("Play"), Summary("Play the provided search term or url"), Alias("p")]
        public async Task Play([Remainder] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await SendWarningAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            var searchResponse = await _lavaNode.SearchAsync(query);

            if (searchResponse.LoadStatus == LoadStatus.LoadFailed || searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                searchResponse = await _lavaNode.SearchYouTubeAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await SendErrorAsync($"I wasn't able to find anything for <{query}>.");
                    return;
                }
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var lavaTrack in searchResponse.Tracks)
                    {
                        var track = lavaTrack;
                        track.Queued = Context.User as IGuildUser;
                        player.Queue.Enqueue(track);
                    }

                    await SendSuccessAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    track.Queued = Context.User as IGuildUser;
                    player.Queue.Enqueue(track);
                    var artwork = await track.FetchArtworkAsync();
                    var embed = new EmbedBuilder()
                        .WithAuthor("Added to queue", Context.User.GetAvatarUrl())
                        .WithTitle(track.Title)
                        .WithUrl(track.Url)
                        .WithThumbnailUrl(artwork)
                        .AddField("Channel", track.Author, true)
                        .AddField("Duration", track.Duration, true)
                        .AddField("Position in queue", player.Queue.Count)
                        .WithFooter($"Requested By {track.Queued.Username}")
                        .WithColor(Color.Teal)
                        .Build();

                    await SendEmbedAsync(embed);
                }
            }
            else
            {
                var track = searchResponse.Tracks[0];
                track.Queued = Context.User as IGuildUser;

                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            var artwork = await track.FetchArtworkAsync();
                            var embed = new EmbedBuilder()
                                .WithAuthor("Now Playing", Context.Client.CurrentUser.GetAvatarUrl())
                                .WithTitle(track.Title)
                                .WithUrl(track.Url)
                                .WithThumbnailUrl(artwork)
                                .AddField("Channel", track.Author, true)
                                .AddField("Duration", track.Duration, true)
                                .WithFooter($"Requested By {track.Queued.Username}")
                                .WithColor(Color.Teal)
                                .Build();

                            await SendEmbedAsync(embed);
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks[i]);
                        }
                    }

                    await SendSuccessAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    await player.PlayAsync(track);
                    var artwork = await track.FetchArtworkAsync();
                    var embed = new EmbedBuilder()
                        .WithAuthor("Now Playing", Context.Client.CurrentUser.GetAvatarUrl())
                        .WithTitle(track.Title)
                        .WithUrl(track.Url)
                        .WithThumbnailUrl(artwork)
                        .AddField("Channel", track.Author, true)
                        .AddField("Duration", track.Duration, true)
                        .WithFooter($"Requested By {track.Queued.Username}")
                        .WithColor(Color.Teal)
                        .Build();

                    await SendEmbedAsync(embed);
                }
            }
        }

        [Command("Pause"), Summary("Pause the bot at the current location")]
        public async Task Pause()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await SendWarningAsync("I cannot pause when I'm not playing anything!");
                return;
            }

            try
            {
                await player.PauseAsync();
                await SendSuccessAsync($"Paused: {player.Track.Title}");
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("Resume"), Summary("Resume the bot")]
        public async Task Resume()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                await SendWarningAsync("I cannot resume when I'm not playing anything!");
                return;
            }

            try
            {
                await player.ResumeAsync();

                var artwork = await player.Track.FetchArtworkAsync();
                var embed = new EmbedBuilder()
                    .WithAuthor("Resumed", Context.User.GetAvatarUrl())
                    .WithTitle(player.Track.Title)
                    .WithUrl(player.Track.Url)
                    .WithThumbnailUrl(artwork)
                    .AddField("Channel", player.Track.Author, true)
                    .AddField("Duration", player.Track.Duration, true)
                    .AddField("Position", player.Track.Position.StripMilliseconds(), true)
                    .WithFooter($"Requested By {player.Track.Queued.Username}")
                    .WithColor(Color.Teal)
                    .Build();

                await SendEmbedAsync(embed);
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("Stop"), Summary("Stop the bot from playing any music")]
        public async Task Stop()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState == PlayerState.Stopped)
            {
                await SendWarningAsync("Woaaah there, I can't stop the stopped forced.");
                return;
            }

            try
            {
                await player.StopAsync();
                await SendWarningAsync("No longer playing anything.");
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("Skip"), Summary("Skip the current song")]
        public async Task Skip()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await SendWarningAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            var voiceChannelUsers = ((SocketVoiceChannel)player.VoiceChannel).Users.Where(x => !x.IsBot).ToArray();
            if (_musicService.VoteQueue.Contains(Context.User.Id) && voiceChannelUsers.Count() != 1)
            {
                await SendErrorAsync("You can't vote again.");
                return;
            }

            _musicService.VoteQueue.Add(Context.User.Id);
            var percentage = _musicService.VoteQueue.Count / voiceChannelUsers.Length * 100;
            if (percentage < 85)
            {
                await SendWarningAsync("You need more than 85% votes to skip this song.");
                return;
            }

            try
            {
                var oldTrack = player.Track;
                var currentTrack = await player.SkipAsync();

                var embed = new EmbedBuilder()
                    .WithAuthor("Skipping Track")
                    .AddField("Skipped", oldTrack.Title)
                    .AddField("Now Playing", currentTrack.Title)
                    .WithColor(Color.Teal)
                    .Build();

                await SendEmbedAsync(embed);
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("Seek"), Summary("Seek within a certain part of a song. Format = 0h0m0s")]
        public async Task Seek(TimeSpan timeSpan)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await SendWarningAsync("Woaaah there, I can't seek when nothing is playing.");
                return;
            }

            try
            {
                await player.SeekAsync(timeSpan);
                await SendSuccessAsync($"I've seeked `{player.Track.Title}` to {timeSpan}.");
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("Volume"), Summary("Set the volume of the bot")]
        public async Task Volume(ushort volume)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            try
            {
                await player.UpdateVolumeAsync(volume);
                await SendSuccessAsync($"I've changed the player volume to {volume}.");
            }
            catch (Exception exception)
            {
                await SendErrorAsync(exception.Message);
            }
        }

        [Command("NowPlaying"), Summary("Display the current track that is being played"), Alias("Np")]
        public async Task NowPlaying()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await SendWarningAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            var embed = new EmbedBuilder();
            embed.WithTitle($"{track.Author} - {track.Title}");
            embed.WithThumbnailUrl(artwork);
            embed.WithUrl(track.Url);
            embed.AddField("Id", track.Id);
            embed.AddField("Duration", track.Duration);
            embed.AddField("Position", track.Position.StripMilliseconds());
            embed.AddField("Remaining", (track.Duration - track.Position).StripMilliseconds());
            embed.WithColor(Color.Teal);
            embed.WithFooter($"Requested By {track.Queued.Username}");

            await SendEmbedAsync(embed.Build());
        }

        //TODO: add the paging system to both of the lyrics systems
        [Command("Genius", RunMode = RunMode.Async), Summary("Display the lyrics from genius site")]
        public async Task ShowGeniusLyrics()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await SendWarningAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await SendErrorAsync($"No lyrics found for {player.Track.Title}");
                return;
            }

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
            {
                if (Range.Contains(stringBuilder.Length))
                {
                    await ReplyAsync($"```{stringBuilder}```");
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }

            await ReplyAsync($"```{stringBuilder}```");
        }

        [Command("OVH", RunMode = RunMode.Async), Summary("Display the lyrics from ovh site")]
        public async Task ShowOvhLyrics()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await SendWarningAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromOVHAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await SendErrorAsync($"No lyrics found for {player.Track.Title}");
                return;
            }

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
            {
                if (Range.Contains(stringBuilder.Length))
                {
                    await ReplyAsync($"```{stringBuilder}```");
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }

            await ReplyAsync($"```{stringBuilder}```");
        }

        [Command("queue", RunMode = RunMode.Async), Summary("View the current list of songs in the queue"), Alias("q")]
        public async Task Queue()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel. So I don't have a queue right now");
                return;
            }

            if (player.Queue.Count < 1)
            {
                await SendWarningAsync("There is currently no songs in the queue.");
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = "Music Queue",
                Color = Color.Teal,
                ThumbnailUrl = await player.Track.FetchArtworkAsync()
            };
            embed.AddField("Now Playing", $"{player.Track.Title} || {player.Track.Queued.Mention}");

            var queue = "";
            for (var index = 0; index < player.Queue.Count; index++)
            {
                queue += $"{index + 1}: {((LavaTrack)player.Queue.ElementAt(index)).Title} || {((LavaTrack)player.Queue.ElementAt(index)).Queued.Username}\n";
            }

            embed.AddField("Up Next", queue);

            await SendEmbedAsync(embed.Build());
        }

        [Command("removequeue", RunMode = RunMode.Async), Summary("Remove a given track from the list using the index"), Alias("rm")]
        public async Task RemoveFromQueue(int index)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendWarningAsync("I'm not connected to a voice channel. So I don't have a queue right now");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await SendErrorAsync("There are currently no items in the queue");
                return;
            }

            if (index < 1 || index > player.Queue.Count)
            {
                await SendErrorAsync("Index out of range, please choose a valid index");
                return;
            }

            player.Queue.RemoveAt(index - 1);

            await SendSuccessAsync("Removed from queue");
        }
    }
}