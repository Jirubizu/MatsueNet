using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MatsueNet.Services;

namespace MatsueNet.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly DatabaseService _databaseService;
        private readonly BalanceService _balanceService;

        public CommandHandler(DiscordShardedClient c, CommandService cs, IServiceProvider s, DatabaseService dbs,
            BalanceService balanceService)
        {
            _client = c;
            _commandService = cs;
            _services = s;
            _databaseService = dbs;
            _balanceService = balanceService;
        }

        public async Task SetupAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _commandService.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            int argPos = 0;
            if (!(msg is SocketUserMessage message))
            {
                return;
            }

            await _balanceService.MessageCoin(message.Author.Id);

            var guildChannel = (SocketGuildChannel) message.Channel;

            var r = await _databaseService.LoadRecordsByGuildId(guildChannel.Guild.Id);

            if (message.HasStringPrefix(r.Prefix, ref argPos))
            {
                var context = new ShardedCommandContext(_client, message);

                var result = await _commandService.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.Message);
            return Task.CompletedTask;
        }
    }
}