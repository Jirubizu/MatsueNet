using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MatsueNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MatsueNet.Attributes.Preconditions
{
    public class ChannelCheckAttribute : PreconditionAttribute
    {
        private readonly Channels[] _channel;

        public ChannelCheckAttribute(params Channels[] channel)
        {
            _channel = channel;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            var db = services.GetRequiredService<DatabaseService>();
            var channel = await db.LoadRecordsByGuildId(context.Guild.Id);

            foreach (var c in _channel)
            {
                switch (c)
                {
                    case Channels.Music:
                        if (channel.MusicChannelId == null)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        else if (channel.MusicChannelId == context.Channel.Id)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }

                        break;
                    case Channels.Bot:
                        if (channel.BotChannelId == null)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        else if (channel.BotChannelId == context.Channel.Id)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }

                        break;
                    case Channels.Admin:
                        if (channel.AdminChannelId == null)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        else if (channel.AdminChannelId == context.Channel.Id)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var message = new StringBuilder();
            message.Append("You must be in ");
            if (channel.AdminChannelId != null)
            {
                message.Append($"<#{channel.AdminChannelId}>, ");
            }

            if (channel.BotChannelId != null)
            {
                message.Append($"<#{channel.BotChannelId}>, ");
            }

            if (channel.MusicChannelId != null)
            {
                message.Append($"<#{channel.MusicChannelId}>, ");
            }

            message.Append($"to use this command");
            return await Task.FromResult(
                PreconditionResult.FromError(message.ToString()));
        }
    }

    public enum Channels
    {
        Music,
        Bot,
        Admin
    }
}