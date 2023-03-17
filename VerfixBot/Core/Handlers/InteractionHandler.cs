using VerfixMusic;

namespace DiscordBot.Core.Handlers;

using System.Reflection;
using VerfixMusic.Core.Managers;
using DiscordBot.Core.Services;
using DiscordBot.Core.Modules;

public class InteractionHandler
{
    private readonly DiscordShardedClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    private readonly LoggingService _loggingService;

    public InteractionHandler(DiscordShardedClient client, InteractionService interactionService, IServiceProvider services, LoggingService loggingService)
    {
        _client = client;
        _interactionService = interactionService;
        _services = services;
        _loggingService = loggingService;
    }

    public async Task InitializeAsync()
    {
        _client.ShardReady += OnReadyAsync;
        _interactionService.Log += OnLogAsync;

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += OnInteractionHandledAsync;
    }

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

    private async Task OnInteractionHandledAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new ShardedInteractionContext(_client, interaction);

            var result = await _interactionService.ExecuteCommandAsync(context, _services);
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) 
                    => await msg.Result.DeleteAsync());
            }
        }
    }
}