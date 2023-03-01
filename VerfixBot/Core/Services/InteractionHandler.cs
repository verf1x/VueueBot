namespace VerfixMusic.Core.Services;

using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;

public class InteractionHandler
{
    private readonly DiscordShardedClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;

    public InteractionHandler(DiscordShardedClient client, InteractionService handler, IServiceProvider services)
    {
        _client = client;
        _handler = handler;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        _client.ShardReady += OnReadyAsync;
        _handler.Log += OnLogAsync;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += OnInteractionHandled;
    }

    private async Task OnLogAsync(LogMessage log)
        => Console.WriteLine(log);

    private async Task OnReadyAsync(DiscordSocketClient shard)
    {
        //if (Program.IsDebug())
        //    await _handler.RegisterCommandsToGuildAsync(_configuration.GetValue<ulong>("testGuild"), true);
        //else
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