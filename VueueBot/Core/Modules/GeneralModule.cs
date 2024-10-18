using System.Text;

namespace VueueBot.Core.Modules;

public sealed class GeneralModule : InteractionModuleBase<ShardedInteractionContext>
{
    public InteractionService Commands { get; set; }

    public GeneralModule()
    {
    }

    [SlashCommand("_pingowner", "respond to ping")]
    [RequireOwner]
    public async Task PingAsync()
    {
        await RespondAsync("Pong!");
    }

    [SlashCommand("help", "info about commands")]
    public async Task HelpAsync()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var command in Commands.SlashCommands)
        {
            sb.Append($"{command.Name}\t-\t{command.Description}\t-\t{command.CommandType}\t-\t{command.Module.Name}" +
                $"\t-\t{command.Module.Parent}\n");
        }

        var embed = new EmbedBuilder
        {
            Fields =
            {
                new EmbedFieldBuilder().WithName("xd").WithValue(sb.ToString()).WithIsInline(false)
            }
        };

        await RespondAsync(embed: embed.Build());
    }

    //[SlashCommand("helloowner", "say hello for my owner")]
    //[RequireOwner]
    //public async Task HelloVerfix()
    //{
    //    await RespondAsync(embed: _customEmbedBuilder
    //        .GetMessageEmbedBuilder($"Hello, my owner!")
    //        .Build());
    //}

    //[SlashCommand("guild_id", "returns guild id")]
    //[RequireOwner]
    //public async Task GetCurrentGuild()
    //{
    //    await RespondAsync(embed: _customEmbedBuilder
    //        .GetMessageEmbedBuilder($"{Context.Guild.Id}")
    //        .Build());
    //}
}