using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using VerfixMusic.Core.Managers;
using VerfixMusic.Core.Services;
using Victoria;
using Victoria.Node;

namespace VerfixMusic;

class Program
{
    private static async Task Main(string[] args)
    {
        var config = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 200,
            TotalShards = 2,
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
                                                   | GatewayIntents.GuildVoiceStates
        };

        using (var services = ConfigureServices(config))
        {
            var client = services.GetRequiredService<DiscordShardedClient>();
            var lavaNode = services.GetRequiredService<LavaNode>();

            client.ShardReady += ReadyAsync;
            client.Log += LogAsync;

            await client.SetGameAsync("$play", type: ActivityType.Listening);

            //await services.GetRequiredService<InteractionHandlingService>()
            //    .InitializeAsync();

            await services.GetRequiredService<CommandHandlingService>()
                .InitializeAsync();

            await lavaNode.ConnectAsync();

            await client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }

    private async Task ReadyAsync()
    {
    }

    private static ServiceProvider ConfigureServices(DiscordSocketConfig config)
            => new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton<CommandService>()
                //.AddSingleton(x => new InteractionHandlingService((IServiceProvider)x.GetRequiredService<DiscordShardedClient>()))
                .AddSingleton<CommandHandlingService>()
                //.AddSingleton<InteractionHandlingService>()
                .AddSingleton<AudioService>()
                .AddSingleton<LavaNode>()
                .AddSingleton<NodeConfiguration>()
                .AddLogging()
                .BuildServiceProvider();


    private static Task ReadyAsync(DiscordSocketClient shard)
    {
        Console.WriteLine($"Shard Number {shard.ShardId} is connected and ready!");
        return Task.CompletedTask;
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}