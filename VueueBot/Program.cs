using System.Text;
using Victoria;
using VueueBot.Core.Handlers;
using VueueBot.Core.Managers;
using VueueBot.Core.Services;

namespace VueueBot;

class Program
{
    private static LavaNode _lavaNode;
    private static DiscordShardedClient _client;
    private static readonly IServiceProvider _provider;
    private static readonly LoggingService _logger;
    private static readonly DiscordSocketConfig _socketConfig;

    static Program()
    {
        _socketConfig = GetSocketConfig();
        _provider = ConfigureServices();
        _logger = _provider.GetRequiredService<LoggingService>();
        _client = _provider.GetRequiredService<DiscordShardedClient>();
        _lavaNode = _provider.GetRequiredService<LavaNode>();

        _client.Log += OnLogAsync;
        _client.ShardReady += OnReady;
    }

    private static async Task Main(string[] args)
    {
        SetUnicodeEncoding();

        await _provider.GetRequiredService<InteractionHandlingService>()
            .InitializeAsync();

        await StartBotAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordShardedClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
                .AddSingleton<InteractionHandlingService>()
                .AddSingleton<AudioService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<EmbedHandler>()
                .AddSingleton<LavaNode>()
                .AddLavaNode()
                .AddLogging()
                .BuildServiceProvider();
    }

    private static async Task OnReady(DiscordSocketClient shard)
    {
        await _lavaNode.ConnectAsync();

        shard.Log += OnLogAsync;
        await _provider.UseLavaNodeAsync();
    }

    private static async Task OnLogAsync(LogMessage log)
    {
        await _logger.LogAsync(log.Source, log.Severity, log.Message, log.Exception);
    }

    private static DiscordSocketConfig GetSocketConfig()
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

    private static async Task StartBotAsync()
    {
        await _client.SetGameAsync("/play", type: ActivityType.Listening);
        await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
        await _client.StartAsync();
    }

    private static void SetUnicodeEncoding()
    {
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;
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