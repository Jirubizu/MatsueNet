using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;
using Victoria.EventArgs;

namespace MatsueNet.Services
{
    public class MusicService
    {
        private readonly LavaNode _lavaNode;
        public readonly HashSet<ulong> VoteQueue;

        public MusicService(LavaNode lNode, DiscordShardedClient sc)
        {
            VoteQueue = new HashSet<ulong>();
            sc.ShardReady += OnReadyAsync;
            _lavaNode = lNode;
            _lavaNode.OnLog += OnLog;
            _lavaNode.OnPlayerUpdated += OnPlayerUpdated;
            _lavaNode.OnStatsReceived += OnStatsReceived;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
        }

        private async Task OnReadyAsync(DiscordSocketClient ci)
        {
            await _lavaNode.ConnectAsync();
        }

        private Task OnLog(LogMessage arg)
        {
            return Task.CompletedTask;
        }

        private Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task OnStatsReceived(StatsEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            // Each time a new track is played clear the voting queue
            VoteQueue.Clear();
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("No more tracks to play.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        private Task OnTrackException(TrackExceptionEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
        {
            return Task.CompletedTask;
        }
    }
}