namespace VerfixMusic.Core.Managers;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordShardedClient _discord;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordShardedClient>();
        _services = services;

        _commands.CommandExecuted += CommandExecutedAsync;
        _commands.Log += LogAsync;
        _discord.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message)
        {
            return;
        }

        if (message.Source != MessageSource.User)
        {
            return;
        }

        var argPos = 0;

        if (!message.HasStringPrefix(ConfigManager.Config.Prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
        {
            return;
        }

        var context = new ShardedCommandContext(_discord, message);
        await _commands.ExecuteAsync(context, argPos, _services);
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified)
        {
            return;
        }

        if (result.IsSuccess)
        {
            return;
        }

        await context.Channel.SendMessageAsync($"error: {result}");
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());

        return Task.CompletedTask;
    }
}