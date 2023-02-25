namespace VerfixMusic.Core.Modules;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using VerfixMusic.Core.Managers;

public class MainModule : InteractionModuleBase<ShardedInteractionContext>
{
    private InteractionService _interactionService;
    private CommandHandlingService _commandHandlingService;

    public MainModule(InteractionService interactionService, CommandHandlingService commandHandlingService)
    {
        _interactionService = interactionService;
        _commandHandlingService = commandHandlingService;
    }

    [SlashCommand("8ball", "find your answer!")]
    public async Task EightBall(string question)
    {
        var replies = new List<string>
        {
            "yes",
            "no",
            "maybe",
            "hazzzzy...."
        };

        var answer = replies[new Random().Next(replies.Count - 1)];

        await RespondAsync($"You asked: [**{question}**], and your answer is: [**{answer}**]");
    }

    [Command("ping")]
    public async Task PingAsync()
    {
        await Context.Channel.TriggerTypingAsync();
        await Context.Channel.SendMessageAsync("Pong!");
    }

    [Command("info")]
    public async Task InfoAsync(SocketGuildUser? socketGuildUser = null)
    {
        if (socketGuildUser == null)
        {
            socketGuildUser = Context.User as SocketGuildUser;
        }

        var embed = new EmbedBuilder()
        {
            Title = $"{socketGuildUser?.Username}#{socketGuildUser?.Discriminator}",
            ThumbnailUrl = socketGuildUser?.GetAvatarUrl() ?? socketGuildUser?.GetDisplayAvatarUrl(),
        }
        .AddField("ID", socketGuildUser?.Id, true)
        .AddField("Name", $"{socketGuildUser?.Username}#{socketGuildUser?.Discriminator}", true)
        .AddField("Created at", socketGuildUser?.CreatedAt, true)
        .Build();

        await ReplyAsync(embed: embed);
    }
}