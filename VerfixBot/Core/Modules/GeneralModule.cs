namespace DiscordBot.Core.Modules.Audio;

using DiscordBot.Core.Handlers;
using System.Text;

public class GeneralModule : InteractionModuleBase<ShardedInteractionContext>
{
    public InteractionService Commands { get; set; }

    public GeneralModule()
    {
    }

    [SlashCommand("help", "info about commands")]
    [RequireOwner]
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