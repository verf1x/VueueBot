namespace VerfixMusic.Core.Modules;

using Discord.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TestModule : ModuleBase<ShardedCommandContext>
{
    private readonly ILogger<TestModule> _logger;
    private readonly IHost _host;

    public TestModule(IHost host, ILogger<TestModule> logger)
    {
        _host = host;
        _logger = logger;
    }

    //[Command("ping")]
    //[Alias("test")]
    //public async Task PingAsync()
    //{
    //    await Context.Channel.TriggerTypingAsync();
    //    await Context.Channel.SendMessageAsync("п-простите, х-хозяин, я уебок((((");
    //}
}
