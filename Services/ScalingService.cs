using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using MatsueNet.Structures;

namespace MatsueNet.Services
{
    public class ScalingService
    {
        private readonly DatabaseService _database;

        public ScalingService(DiscordShardedClient client, DatabaseService db)
        {
            _database = db;
            client.UserVoiceStateUpdated += UserJoinLeave;
        }

        public async Task UserJoinLeave(SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            // When a person leaves a channel check if the there are any more players there
            // When making a new channel set it to temp
            // WHen a person joins a channel if it gets full make a new one
            // oldVoiceState = Left channel e.g. moving to a new channel or disconnecting
            // newVoiceState = Joined channel e.g. moved in to a channel

            var newVc = newVoiceState.VoiceChannel;
            var oldVc = oldVoiceState.VoiceChannel;

            // Leaving a channel
            try
            {
                if (oldVc.Name != null)
                {
                    var guild = await _database.LoadRecordsByGuildId(oldVc.Guild.Id);
                    guild.ScaledChannels ??= new List<SocketVoiceChannelBson>();
                    if (guild.ScaledChannels.Any(channel => channel.Id == oldVc.Id))
                    {
                        foreach (var temp in guild.ScaledChannels.SelectMany(c => c.TempChannels.Where(temp => temp.Id == oldVc.Id && oldVc.Users.Count == 0)))
                        {
                            await oldVc.Guild.GetVoiceChannel(temp.Id).DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            // Joining a new channel
            Console.WriteLine(newVc.Name);
            if (newVc.Name != null)
            {
                var guild = await _database.LoadRecordsByGuildId(newVc.Guild.Id);
                guild.ScaledChannels ??= new List<SocketVoiceChannelBson>();

                if (guild.ScaledChannels.Any(channel => channel.Id == newVc.Id) && newVc.Users.Count == newVc.UserLimit)
                {
                    var channel = await newVc.Guild.CreateVoiceChannelAsync(newVc.Name + ">Scaled<", v =>
                    {
                        v.Bitrate = newVc.Bitrate;
                        v.UserLimit = newVc.UserLimit;
                        v.Name = newVc.Name;
                        v.Position = newVc.Position + 1;
                        v.CategoryId = newVc.CategoryId;
                    });

                    foreach (var cha in guild.ScaledChannels.Where(cha => cha.Id == newVc.Id))
                    {
                        cha.TempChannels ??= new List<SocketVoiceChannelBson>();
                        cha.TempChannels.Add(new SocketVoiceChannelBson
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            Position = channel.Position,
                            Bitrate = channel.Bitrate,
                            UserLimit = channel.UserLimit,
                            CategoryId = channel.CategoryId
                        });
                        await _database.UpdateGuild(guild);
                    }
                }
            }
        }

        public async Task ScaleChannel(SocketVoiceChannel voiceChannel)
        {
            var guild = await _database.LoadRecordsByGuildId(voiceChannel.Guild.Id);
            guild.ScaledChannels ??= new List<SocketVoiceChannelBson>();
            guild.ScaledChannels.Add(new SocketVoiceChannelBson
            {
                Id = voiceChannel.Id,
                TempChannels = new List<SocketVoiceChannelBson>()
            });
            await _database.UpdateGuild(guild);
        }

        public async Task DeScaleChannel(SocketVoiceChannel voiceChannel)
        {
            var guild = await _database.LoadRecordsByGuildId(voiceChannel.Guild.Id);
            guild.ScaledChannels.Remove(guild.ScaledChannels.First(g => g.Id == voiceChannel.Id));
            await _database.UpdateGuild(guild);
        }
    }
}