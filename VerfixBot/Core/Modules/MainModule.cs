namespace VerfixMusic.Core.Modules;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

public class MainModule : ModuleBase<ShardedCommandContext>
{
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