namespace VerfixMusic.Core.Services;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Node;

public class InteractionHandlingService
{
    private readonly InteractionService _service;
    private readonly DiscordShardedClient _client;
    private readonly IServiceProvider _provider;
    private readonly LavaNode _lavaNode;

    public InteractionHandlingService(IServiceProvider services)
    {
        _service = services.GetRequiredService<InteractionService>();
        _client = services.GetRequiredService<DiscordShardedClient>();
        _provider = services;
        _lavaNode = services.GetRequiredService<LavaNode>();

        _service.Log += LogAsync;
        _client.InteractionCreated += OnInteractionAsync;
        _client.ShardReady += ReadyAsync;
    }

    public async Task InitializeAsync()
    {
        await _service.AddModulesAsync(typeof(InteractionHandlingService).Assembly, _provider);
    }

    private async Task OnInteractionAsync(SocketInteraction interaction)
    {
        _ = Task.Run(async () =>
        {
            var context = new ShardedInteractionContext(_client, interaction);
            await _service.ExecuteCommandAsync(context, _provider);
        });
        await Task.CompletedTask;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());

        return Task.CompletedTask;
    }

    private async Task ReadyAsync(DiscordSocketClient _)
    {
#if DEBUG
        await _service.RegisterCommandsToGuildAsync(1 /* implement */);
#else
            await _service.RegisterCommandsGloballyAsync();
#endif
    }
}
