namespace VerfixMusic;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using VerfixMusic.Core.Managers;
using VerfixMusic.Core.Services;
using Victoria.Node;
using RunMode = Discord.Commands.RunMode;

class Program
{
    private static LavaNode? _lavaNode;

    private static async Task Main(string[] args)
    {
        var config = new DiscordSocketConfig
        {
#if DEBUG
            LogLevel = LogSeverity.Debug,
#else
            LogLevel = LogSeverity.Verbose,
#endif
            AlwaysDownloadUsers = true,
            MessageCacheSize = 200,
            TotalShards = 2,
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMembers|
                             GatewayIntents.GuildVoiceStates
        };

        using (var services = ConfigureServices(config))
        {
            var client = services.GetRequiredService<DiscordShardedClient>();
            _lavaNode = services.GetRequiredService<LavaNode>();

            client.ShardReady += ReadyAsync;
            client.Log += LogAsync;

            await client.SetGameAsync($"{ConfigManager.Config.Prefix}play", type: ActivityType.Listening);

            await services.GetRequiredService<InteractionHandlingService>()
                .InitializeAsync();

            await services.GetRequiredService<CommandHandlingService>()
                .InitializeAsync();

            await client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }

    private static ServiceProvider ConfigureServices(DiscordSocketConfig config)
            => new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
#if DEBUG
                    LogLevel = LogSeverity.Debug,
#else
                    LogLevel = LogSeverity.Verbose,
#endif
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    IgnoreExtraArgs = true,
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<InteractionHandlingService>()
                .AddSingleton<AudioService>()
                .AddSingleton<LavaNode>()
                .AddSingleton<NodeConfiguration>()
                .AddLogging()
                .BuildServiceProvider();

    private static Task ReadyAsync(DiscordSocketClient shard)
    {
        _lavaNode?.ConnectAsync();

        Console.WriteLine($"Shard Number {shard.ShardId} is connected and ready!");
        return Task.CompletedTask;
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}