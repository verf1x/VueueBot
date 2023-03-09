namespace VerfixMusic;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Core.Handlers;
using DiscordBot.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VerfixMusic.Core.Managers;
using Victoria.Node;

class Program
{
    private LavaNode? _lavaNode;
    private DiscordShardedClient? _client;
    private readonly IServiceProvider _services;
    private readonly LoggingService _logger;
    private readonly DiscordSocketConfig _socketConfig = new()
    {
        LogLevel = IsDebug() ? LogSeverity.Debug :LogSeverity.Info,
        AlwaysDownloadUsers = true,
        TotalShards = 1,
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMembers |
                             GatewayIntents.GuildVoiceStates
    };

    public Program()
    {
        _services = ConfigureServices();
        _logger = _services.GetRequiredService<LoggingService>();
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

        //await StartLavalinkExecAsync();

        await Task.Delay(Timeout.Infinite);
    }

//    private async Task StartLavalinkExecAsync()
//    {
//#pragma warning disable CS8602
//        var lavalinkPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())
//            .Parent
//            .Parent
//            .Parent
//            .FullName);
//#pragma warning restore CS8602

//        var processStartInfo = new ProcessStartInfo
//        {
//            FileName = "powershell.exe",
//            WorkingDirectory = lavalinkPath,
//            Arguments = $"Java -jar Lavalink.jar"
//        };

//        Process.Start(processStartInfo);

//        await Task.Delay(-1);
//    }

    private ServiceProvider ConfigureServices()
            => new ServiceCollection()
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

    private Task OnReadyAsync(DiscordSocketClient shard)
    {
        _lavaNode?.ConnectAsync();

        shard.Log += OnLogAsync;
        return Task.CompletedTask;
    }

    private async Task OnLogAsync(LogMessage log)
    {
        await _logger.LogAsync(log.Source, log.Severity, log.Message);
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