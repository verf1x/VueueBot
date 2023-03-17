namespace VerfixMusic.Core.Managers;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

public class AudioService
{
    private readonly LavaNode _lavaNode;
    private readonly ILogger _logger;
    public readonly HashSet<ulong> VoteQueue;
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

    public AudioService(LavaNode lavaNode, ILoggerFactory loggerFactory)
    {
        _lavaNode = lavaNode;
        _logger = loggerFactory.CreateLogger<LavaNode>();
        _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        VoteQueue = new HashSet<ulong>();

        _lavaNode.OnTrackEnd += OnTrackEndAsync;
        _lavaNode.OnTrackStart += OnTrackStart;
        _lavaNode.OnStatsReceived += OnStatsReceived;
        _lavaNode.OnUpdateReceived += OnUpdateReceived;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnTrackException += OnTrackException;
    }

    private static Task OnTrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        //arg.Player.Vueue.Enqueue(arg.Track);
        //return arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        return Task.CompletedTask;
    }

    private static Task OnTrackStuck(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        //arg.Player.Vueue.Enqueue(arg.Track);
        //return arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        return Task.CompletedTask;
    }

    private Task OnWebSocketClosed(WebSocketClosedEventArg arg)
    {
        _logger.LogCritical($"{arg.Code} {arg.Reason}");
        return Task.CompletedTask;
    }

    private Task OnStatsReceived(StatsEventArg arg)
    {
        _logger.LogInformation(JsonSerializer.Serialize(arg));
        return Task.CompletedTask;
    }

    private static Task OnUpdateReceived(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnTrackStart(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return Task.CompletedTask;
    }

    private async Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        if (!(arg.Reason == TrackEndReason.Finished))
        {
            await Task.CompletedTask;
            return;
        }

        var player = arg.Player;

        if (!player.Vueue.TryDequeue(out var queueable))
        {
            return;
        }

        if (queueable is not LavaTrack track)
        {
            return;
        }
        await arg.Player.PlayAsync(track);

        await Task.CompletedTask;
        return;
    }
}
