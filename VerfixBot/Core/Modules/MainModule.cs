namespace VerfixMusic.Core.Modules;

using Discord.Interactions;
using VerfixMusic.Common;
using Discord;
using DiscordBot.Core.Handlers;

public class MainModule : InteractionModuleBase<ShardedInteractionContext>
{
    public InteractionService? Commands { get; set; }
    private InteractionHandler? _handler;
    private CustomEmbedBuilder _customEmbedBuilder;

    public MainModule(InteractionHandler handler)
    {
        _handler = handler;
        _customEmbedBuilder = new CustomEmbedBuilder();
    }

    [SlashCommand("ping", "checks the status of the bot")]
    public async Task PingAsync()
    {
        var embed = new CustomEmbedBuilder()
        {
            Title = "Pong!"
        };

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("helloowner", "say hello for my owner")]
    [RequireOwner]
    public async Task HelloVerfix()
    {
        await RespondAsync(embed: _customEmbedBuilder
            .GetMessageEmbedBuilder($"Hello, my owner!")
            .Build());
    }

    [SlashCommand("guild_id", "returns guild id")]
    [RequireOwner]
    public async Task GetCurrentGuild()
    {
        await RespondAsync(embed: _customEmbedBuilder
            .GetMessageEmbedBuilder($"{Context.Guild.Id}")
            .Build());
    }
}