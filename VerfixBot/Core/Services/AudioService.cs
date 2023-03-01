namespace VerfixMusic.Core.Managers;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using VerfixMusic.Common;
using Victoria;
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
        _lavaNode.OnTrackStart += OnTrackStartAsync;
        _lavaNode.OnStatsReceived += OnStatsReceivedAsync;
        _lavaNode.OnUpdateReceived += OnUpdateReceivedAsync;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
        _lavaNode.OnTrackStuck += OnTrackStuckAsync;
        _lavaNode.OnTrackException += OnTrackExceptionAsync;
    }

    private static Task OnTrackExceptionAsync(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        var embed = new VerfixEmbedBuilder();
        embed.Title = $"{arg.Track} has been requeued because it threw an exception.";

        arg.Player.Vueue.Enqueue(arg.Track);
        return arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
    }

    private static Task OnTrackStuckAsync(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        var embed = new VerfixEmbedBuilder();
        embed.Title = $"{arg.Track} has been requeued because it got stuck.";

        arg.Player.Vueue.Enqueue(arg.Track);
        return arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
    }

    private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
    {
        _logger.LogCritical($"{arg.Code} {arg.Reason}");
        return Task.CompletedTask;
    }

    private Task OnStatsReceivedAsync(StatsEventArg arg)
    {
        _logger.LogInformation(JsonSerializer.Serialize(arg));
        return Task.CompletedTask;
    }

    private static Task OnUpdateReceivedAsync(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return Task.CompletedTask;
    }

    private static Task OnTrackStartAsync(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return Task.CompletedTask;
    }

    private async Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        var embed = new VerfixEmbedBuilder();

        if (!(arg.Reason == TrackEndReason.Finished))
        {
            await Task.CompletedTask;
            return;
        }

        var player = arg.Player;

        if (!player.Vueue.TryDequeue(out var queueable))
        {
            embed.Title = "Queue completed! Please add more tracks to rock n' roll!";

            await player.TextChannel.SendMessageAsync(embed: embed.Build());
            return;
        }

        if (!(queueable is LavaTrack track))
        {
            await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        }

        var artwork = track.FetchArtworkAsync();

        embed.Title = $"Now Playing:";
        embed.WithImageUrl(artwork.Result);
        embed.AddField($"{track?.Title}", track?.Url, true);
         
        await arg.Player.PlayAsync(track);
        await arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());

        await Task.CompletedTask;
        return;
    }
}
