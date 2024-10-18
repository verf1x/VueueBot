using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Victoria;
using Victoria.Enums;
using Victoria.WebSocket.EventArgs;
using VueueBot.Core.Services;

namespace VueueBot.Core.Managers;

public class AudioService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LavaNode _lavaNode;
    //private readonly ILogger _logger;
    private readonly LoggingService _logger;
    public readonly HashSet<ulong> VoteQueue;
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
    public readonly ConcurrentDictionary<ulong, ulong> TextChannels;
    private readonly DiscordShardedClient _client;

    public AudioService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _lavaNode = serviceProvider.GetRequiredService<LavaNode>();
        //_logger = loggerFactory.CreateLogger<LavaNode>();
        _logger = serviceProvider.GetRequiredService<LoggingService>();
        _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        TextChannels = new ConcurrentDictionary<ulong, ulong>();
        VoteQueue = new HashSet<ulong>();

        SubscribeLavaNodeEvents();
    }

    private void SubscribeLavaNodeEvents()
    {
        _lavaNode.OnTrackEnd += OnTrackEndAsync;
        _lavaNode.OnTrackStart += OnTrackStart;
        _lavaNode.OnStats += OnStats;
        _lavaNode.OnPlayerUpdate += OnPlayerUpdate;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnTrackException += OnTrackException;
    }

    private async Task OnTrackStart(TrackStartEventArg arg)
    {
        await Task.CompletedTask;
    }

    private async Task OnTrackEndAsync(TrackEndEventArg arg)
    {
        if (!(arg.Reason == TrackEndReason.Finished))
        {
            await Task.CompletedTask;
            return;
        }

        var player = await _lavaNode.TryGetPlayerAsync(_client.GetGuild(arg.GuildId).Id);

        if (!player.GetQueue().TryDequeue(out var queueable))
        {
            return;
        }

        if (queueable is not LavaTrack track)
        {
            return;
        }

        await player.PlayAsync(_lavaNode, track);
    }

    private async Task OnTrackException(TrackExceptionEventArg arg)
    {
        await Task.CompletedTask;
    }

    private async Task OnTrackStuck(TrackStuckEventArg arg)
    {
        //arg.Player.Vueue.Enqueue(arg.Track);
        //return arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        await Task.CompletedTask;
    }

    private async Task OnWebSocketClosed(WebSocketClosedEventArg arg)
    {
        await _logger.LogAsync("audio", LogSeverity.Info, $"Websocket Closed: {arg.Reason}");
    }

    private async Task OnStats(StatsEventArg arg)
    {
        await _logger.LogAsync("audio", LogSeverity.Info,$"{JsonSerializer.Serialize(arg)}");
        await Task.CompletedTask;
    }

    private async Task OnPlayerUpdate(PlayerUpdateEventArg arg)
    {
        await Task.CompletedTask;
    }
}
