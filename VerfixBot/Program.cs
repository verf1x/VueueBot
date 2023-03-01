namespace VerfixMusic;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using VerfixMusic.Core.Managers;
using VerfixMusic.Core.Services;
using Victoria.Node;

class Program
{
    private LavaNode? _lavaNode;
    private DiscordShardedClient? _client;
    private readonly IServiceProvider _services;
    private readonly DiscordSocketConfig _socketConfig = new()
    {
        LogLevel = IsDebug() ? LogSeverity.Debug :LogSeverity.Info,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 200,
        TotalShards = 2,
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMembers |
                             GatewayIntents.GuildVoiceStates
    };

    public Program()
    {
        _services = ConfigureServices();
    }

    static void Main(params string[] args)
        => new Program().MainAsync()
            .GetAwaiter()
            .GetResult();

    private async Task MainAsync()
    {
        _client = _services.GetRequiredService<DiscordShardedClient>();
        _lavaNode = _services.GetRequiredService<LavaNode>();

        _client.Log += OnLogAsync;
        _client.ShardReady += OnReadyAsync;

        await _client.SetGameAsync("/play", type: ActivityType.Listening);

        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton<DiscordShardedClient>()
                .AddSingleton(_socketConfig)
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<AudioService>()
                .AddSingleton<LavaNode>()
                .AddSingleton<NodeConfiguration>()
                .AddLogging()
                .BuildServiceProvider();

    private Task OnReadyAsync(DiscordSocketClient shard)
    {
        _lavaNode?.ConnectAsync();

        Console.WriteLine($"Shard Number {shard.ShardId} is connected and ready!");
        return Task.CompletedTask;
    }

    private Task OnLogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}