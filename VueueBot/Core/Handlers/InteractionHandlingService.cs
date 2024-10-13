using System.Reflection;
using VueueBot.Core.Managers;
using VueueBot.Core.Services;

namespace VueueBot.Core.Handlers;

public class InteractionHandlingService
{
    private readonly DiscordShardedClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _provider;
    private readonly LoggingService _loggingService;

    public InteractionHandlingService(IServiceProvider services)
    {
        _client = services.GetRequiredService<DiscordShardedClient>();
        _interactionService = services.GetRequiredService<InteractionService>();
        _loggingService = services.GetRequiredService<LoggingService>();
        _provider = services;

        _client.ShardReady += OnReadyAsync;
        _interactionService.Log += OnLogAsync;
        _client.InteractionCreated += OnInteractionAsync;
    }

    public async Task InitializeAsync()
        => await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

    private async Task OnLogAsync(LogMessage log) 
        => await _loggingService.LogAsync(log.Source, log.Severity, log.Message);

    private async Task OnReadyAsync(DiscordSocketClient shard)
    {
        if (Program.IsDebug())
        {
            await _interactionService.RegisterCommandsToGuildAsync(ConfigManager.Config.TestGuildId, true);
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync(true);
        }
    }

    private async Task OnInteractionAsync(SocketInteraction interaction)
    {
        _ = Task.Run(async () =>
        {
            var context = new ShardedInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, _provider);
        });

        await Task.CompletedTask;
    }
}