namespace VerfixMusic.Core.Managers;

using Discord;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using VerfixMusic.Common;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;

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

    private static Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        var embed = new VerfixEmbedBuilder();

        if (!(arg.Reason == TrackEndReason.Finished))
        {
            return Task.CompletedTask;
        }

        var player = arg.Player;

        if (!player.Vueue.TryDequeue(out var queueable))
        {
            embed.Title = "Queue completed! Please add more tracks to rock n' roll!";

            player.TextChannel.SendMessageAsync(embed: embed.Build());
            return Task.CompletedTask;
        }

        if (!(queueable is LavaTrack track))
        {
            player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return Task.CompletedTask;
        }

        var artwork = track.FetchArtworkAsync();

        embed.Title = $"Now Playing:";
        embed.WithImageUrl(artwork.Result);
        embed.AddField($"{track?.Title}", track?.Url, true);

        arg.Player.PlayAsync(track);
        arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());

        return Task.CompletedTask;
    }
}
