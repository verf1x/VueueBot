namespace VerfixMusic.Core.Modules;

using Discord.Commands;

public class MainModule : ModuleBase<ShardedCommandContext>
{
    [Command("ping")]
    [Alias("test")]
    public async Task PingAsync()
    {
        await Context.Channel.TriggerTypingAsync();
        await Context.Channel.SendMessageAsync("п-простите, х-хозяин, я уебок((((");
    }
}