namespace VerfixMusic;

using DiscordBot.Core.Handlers;
using DiscordBot.Core.Services;
using VerfixMusic.Core.Managers;
using Victoria.Node;

class Program
{
    private LavaNode _lavaNode;
    private DiscordShardedClient _client;
    private readonly IServiceProvider _services;
    private readonly LoggingService _logger;
    private readonly DiscordSocketConfig _socketConfig;

    public Program()
    {
        _socketConfig = GetSocketConfig();
        _services = ConfigureServices();
        _logger = _services.GetRequiredService<LoggingService>();
    }

    static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private async Task MainAsync()
    {
        _client = _services.GetRequiredService<DiscordShardedClient>();
        _lavaNode = _services.GetRequiredService<LavaNode>();

        _client.Log += OnLogAsync;
        _client.ShardReady += OnReady;

        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        await _client.SetGameAsync("/play", type: ActivityType.Listening);
        await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordShardedClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<AudioService>()
                .AddSingleton<LavaNode>()
                .AddSingleton<NodeConfiguration>()
                .AddSingleton<LoggingService>()
                .AddSingleton<EmbedHandler>()
                .AddLogging()
                .BuildServiceProvider();
    }

    private Task OnReady(DiscordSocketClient shard)
    {
        _lavaNode?.ConnectAsync();

        shard.Log += OnLogAsync;
        return Task.CompletedTask;
    }

    private async Task OnLogAsync(LogMessage log)
    {
        await _logger.LogAsync(log.Source, log.Severity, log.Message);
    }

    private DiscordSocketConfig GetSocketConfig()
    {
        return new()
        {
            LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Info,
            AlwaysDownloadUsers = true,
            TotalShards = 1,
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent |
                             GatewayIntents.GuildMembers | GatewayIntents.GuildVoiceStates
        };
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