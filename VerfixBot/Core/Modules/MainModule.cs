namespace VerfixMusic.Core.Modules;

using VerfixMusic.Core.Services;
using Discord.Interactions;
using VerfixMusic.Common;

public class MainModule : InteractionModuleBase<ShardedInteractionContext>
{
    public InteractionService? Commands { get; set; }

    private InteractionHandler? _handler;

    public MainModule(InteractionHandler handler)
    {
        _handler = handler;
    }

    [SlashCommand("ping", "checks the status of the bot")]
    public async Task PingAsync()
    {
        var embed = new VerfixEmbedBuilder()
        {
            Title = "Pong!"
        };

        await RespondAsync(embed: embed.Build());
    }
}