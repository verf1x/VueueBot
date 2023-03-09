using VerfixMusic;

namespace DiscordBot.Core.Handlers;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using VerfixMusic.Core.Managers;
using DiscordBot.Core.Services;

public class InteractionHandler
{
    private readonly DiscordShardedClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly LoggingService _loggingService;

    public InteractionHandler(DiscordShardedClient client, InteractionService handler, IServiceProvider services, LoggingService loggingService)
    {
        _client = client;
        _handler = handler;
        _services = services;
        _loggingService = loggingService;
    }

    public async Task InitializeAsync()
    {
        _client.ShardReady += OnReadyAsync;
        _handler.Log += OnLog;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += OnInteractionHandled;
    }

    private async Task OnLog(LogMessage log)
    {
        await _loggingService.LogAsync(log.Source, log.Severity, log.Message);
    }

    private async Task OnReadyAsync(DiscordSocketClient shard)
    {
        if (Program.IsDebug())
            await _handler.RegisterCommandsToGuildAsync(ConfigManager.Config.TestGuildId, true);
        else
            await _handler.RegisterCommandsGloballyAsync(true);
    }

    private async Task OnInteractionHandled(SocketInteraction interaction)
    {
        try
        {
            var context = new ShardedInteractionContext(_client, interaction);

            var result = await _handler.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        break;
                }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}