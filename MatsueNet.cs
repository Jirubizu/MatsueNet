using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using MatsueNet.Handlers;
using MatsueNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Miki.Anilist;

namespace MatsueNet
{
    public class MatsueNet
    {
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private ConfigService _config;
        private LavaConfig _lavaConfig;
        
        public MatsueNet(DiscordShardedClient client = null, CommandService commandService = null)
        {
            _client = client ?? new DiscordShardedClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Verbose
            });

            _commandService = commandService ?? new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });
        }

        public async Task SetupAsync()
        {
            _config = new ConfigService("./config.json");
            
            await _client.LoginAsync(TokenType.Bot, _config.Config.BotToken);
            await _client.StartAsync();
            
            _lavaConfig = new LavaConfig{Authorization = _config.Config.LavaLinkPassword, Hostname = _config.Config.LavaLinkHostname, Port = _config.Config.LavaLinkPort};
            _client.Log += LogAsync;

            var services = SetupServices();
            
            var commandHandler = services.GetRequiredService<CommandHandler>();
            await commandHandler.SetupAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider SetupServices() => new ServiceCollection()
            .AddSingleton(this)
            .AddSingleton(_client)
            .AddSingleton(_commandService)
            .AddSingleton(_config)
            .AddSingleton<CommandHandler>()
            .AddSingleton<HttpService>()
            .AddSingleton<RandomService>()
            .AddSingleton<MusicService>()
            .AddSingleton<LavaNode>()
            .AddSingleton(_lavaConfig)
            .AddSingleton<AnilistClient>()
            .AddSingleton<MinecraftService>()
            .AddSingleton<DatabaseService>()
            .AddSingleton<OsuService>()
            .AddSingleton<TronaldDumpService>()
            .AddSingleton<BalanceService>()
            .AddSingleton(new InteractivityService(_client, TimeSpan.FromMinutes(3)))
            .BuildServiceProvider();

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.Message);
            return Task.CompletedTask;
        }
    }
}