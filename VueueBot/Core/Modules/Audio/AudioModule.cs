using Discord.Interactions;
using System.Text;
using Victoria;
using VueueBot.Core.Handlers;
using VueueBot.Core.Managers;

namespace VueueBot.Core.Modules.Audio;

public class AudioModule(
    LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode,
    AudioService _audioService,
    EmbedHandler _embedHandler) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly IEnumerable<int> _range = Enumerable.Range(1900, 2000);

    public InteractionService Commands { get; set; }

    [SlashCommand("join", "Joins to the voice channel")]
    public async Task JoinAsync()
    {
        var voiceState = Context.User as IVoiceState;

        if (voiceState?.VoiceChannel is null)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"You must be connected to a voice channel!"));
            return;
        }
        try
        {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel);
            await RespondAsync(embed: await _embedHandler.CreateSuccessJoinEmbedAsync(voiceState.VoiceChannel.Name, voiceState.VoiceChannel.Bitrate));

            _audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception));
        }
    }

    [SlashCommand("leave", "Leave a voice channel")]
    public async Task LeaveAsync()
    {
        var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel;
        if (voiceChannel is null)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("Not sure which voice channel to disconnect from."));
            return;
        }

        try
        {
            await _lavaNode.LeaveAsync(voiceChannel);
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"I've left {voiceChannel.Name}!"));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception));
        }
    }

    //[SlashCommand("play", "Play a song")]
    //public async Task PlayAsync(string searchQuery)
    //{
    //    if (string.IsNullOrWhiteSpace(searchQuery))
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("Please provide search terms."));
    //        return;
    //    }

    //    if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
    //    {
    //        var voiceState = Context.User as IVoiceState;
    //        if (voiceState?.VoiceChannel is null)
    //        {
    //            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("You must be connected to a voice channel!"));
    //            return;
    //        }

    //        try
    //        {
    //            player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);

    //        }
    //        catch (Exception exception)
    //        {
    //            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
    //        }
    //    }

    //    var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, searchQuery);
    //    if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"I wasn't able to find anything for `{searchQuery}`."));
    //        return;
    //    }

    //    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
    //    {
    //        player.Vueue.Enqueue(searchResponse.Tracks);
    //        await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Enqueued {searchResponse.Tracks.Count} songs."));
    //    }
    //    else
    //    {
    //        var track = searchResponse.Tracks.FirstOrDefault();
    //        player.Vueue.Enqueue(track);

    //        var artwork = await track.FetchArtworkAsync();

    //        await RespondAsync(embed: await _embedHandler.CreateMediaEmbedAsync("Added to playlist:", artwork, track.Title, track.Url));
    //    }

    //    if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
    //    {
    //        return;
    //    }

    //    player.Vueue.TryDequeue(out var lavaTrack);
    //    await player.PlayAsync(lavaTrack);
    //    await player.SetVolumeAsync(30);
    //}

    //[SlashCommand("pause", "Pause playing song", runMode: RunMode.Async)]
    //public async Task PauseAsync()
    //{
    //    var player = await TryGetLavaPlayer();
    //    if (player is null)
    //    {
    //        return;
    //    }

    //    if (player.PlayerState is not PlayerState.Playing)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("I cannot pause when I'm not playing anything!"));
    //        return;
    //    }

    //    try
    //    {
    //        await player.PauseAsync();
    //        await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Paused: {player.Track.Title}"));
    //    }
    //    catch (Exception exception)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
    //    }
    //}

    //[SlashCommand("resume", "Resume paused track", runMode: RunMode.Async)]
    //public async Task ResumeAsync()
    //{
    //    var player = await TryGetLavaPlayer();
    //    if (player is null)
    //    {
    //        return;
    //    }

    //    if (player.PlayerState is not PlayerState.Paused)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("I cannot resume when I'm not playing anything!"));
    //        return;
    //    }

    //    try
    //    {
    //        await player.ResumeAsync();
    //        await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Resumed: {player.Track.Title}"));
    //    }
    //    catch (Exception exception)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
    //    }
    //}

    //[SlashCommand("stop", "Stop playing song", runMode: RunMode.Async)]
    //public async Task StopAsync()
    //{
    //    var player = await TryGetLavaPlayer();
    //    if (player is null)
    //    {
    //        return;
    //    }

    //    if (!await IsPlayerPlaying(player))
    //    {
    //        return;
    //    }

    //    try
    //    {
    //        await player.StopAsync();
    //        await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync("No longer playing anything."));
    //    }
    //    catch (Exception exception)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
    //    }
    //}

    //[SlashCommand("skip", "Skip a song", runMode: RunMode.Async)]
    //public async Task SkipAsync()
    //{
    //    var player = await TryGetLavaPlayer();
    //    if (player is null)
    //    {
    //        return;
    //    }

    //    if (!await IsPlayerPlaying(player))
    //    {
    //        return;
    //    }

    //    var voiceChannelUsers = ((SocketGuild)player.VoiceChannel.Guild).Users
    //        .Where(x => !x.IsBot)
    //        .ToArray();
    //    if (_audioService.VoteQueue.Contains(Context.User.Id))
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("You can't vote again."));
    //        return;
    //    }

    //    _audioService.VoteQueue.Add(Context.User.Id);
    //    var percentage = _audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
    //    if (percentage <= 50)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("You need more than 50% votes to skip this song."));
    //        return;
    //    }

    //    try
    //    {
    //        var (skipped, currenTrack) = await player.SkipAsync();
    //        await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Skipped: {skipped.Title}"));
    //    }
    //    catch (Exception exception)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
    //    }
    //}

    ////[SlashCommand("seek", "VerfixMusic seek timeStamp", runMode: RunMode.Async)]
    ////public async Task SeekAsync(TimeSpan timeSpan)
    ////{
    ////    var embedBuilder = new CustomEmbedBuilder();

    ////    var player = await TryGetLavaPlayer(embedBuilder);
    ////    if (player == null)
    ////    {
    ////        return;
    ////    }

    ////    if (!await IsPlayerPlaying(embedBuilder, player))
    ////    {
    ////        return;
    ////    }

    ////    try
    ////    {
    ////        embedBuilder.Title = $"I've seeked `{player.Track.Title}` to {timeSpan}.";

    ////        await player.SeekAsync(timeSpan);
    ////        await RespondAsync(embed: embedBuilder.Build());
    ////    }
    ////    catch (Exception exception)
    ////    {
    ////        await CallException(embedBuilder, exception);
    ////    }
    ////}

    //[SlashCommand("volume", "Change song volume", runMode: RunMode.Async)]
    //public async Task SetVolumeAsync(ushort volume)
    //{
    //    var player = await TryGetLavaPlayer();
    //    if (player is null)
    //    {
    //        return;
    //    }

    //    try
    //    {
    //        await player.SetVolumeAsync(volume);
    //        await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"I've changed the player volume to {volume}."));
    //    }
    //    catch (Exception exception)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
    //    }
    //}

    //[SlashCommand("nowplaying", "Show what is currently playing")]
    //public async Task NowPlayingAsync()
    //{
    //    var player = await TryGetLavaPlayer();
    //    if (player is null)
    //    {
    //        return;
    //    }

    //    if (!await IsPlayerPlaying(player))
    //    {
    //        return;
    //    }

    //    var track = player.Track;
    //    var artwork = await track.FetchArtworkAsync();

    //    await RespondAsync(embed: await _embedHandler.CreateMediaEmbedAsync($"Now Playing:", artwork, track.Title, track.Url));
    //}

    ////[SlashCommand("genius", "genius lyrics for current song")]
    ////public async Task ShowGeniusLyrics()
    ////{
    ////    var player = await TryGetLavaPlayer();
    ////    if (player is null)
    ////    {
    ////        return;
    ////    }

    ////    if (!await IsPlayerPlaying(player))
    ////    {
    ////        return;
    ////    }

    ////    var lyrics = await LyricsResolver.SearchOvhAsync(player.Track);
    ////    if (string.IsNullOrWhiteSpace(lyrics))
    ////    {
    ////        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"No lyrics found for {player.Track.Title}"));
    ////        return;
    ////    }

    ////    var splitLyrics = lyrics.Split(Environment.NewLine);
    ////    var stringBuilder = new StringBuilder();
    ////    foreach (var line in splitLyrics)
    ////    {
    ////        if (_range.Contains(stringBuilder.Length))
    ////        {
    ////            await RespondAsync($"```{stringBuilder}```");
    ////            stringBuilder.Clear();
    ////        }
    ////        else
    ////        {
    ////            stringBuilder.AppendLine(line.TrimEnd('\n'));
    ////        }
    ////    }

    ////    //embedBuilder.Title = $"Genius Lyrics";

    ////    await RespondAsync($"```{stringBuilder}```");
    ////}

    ////[SlashCommand("loop", "Loop track", runMode: RunMode.Async)]
    ////public async Task LoopCurrentTrack(int loops)
    ////{
    ////    loops--;

    ////    var player = await TryGetLavaPlayer();
    ////    if (player is null)
    ////    {
    ////        return;
    ////    }

    ////    if (!await IsPlayerPlaying(player))
    ////    {
    ////        return;
    ////    }

    ////    if (loops <= 0)
    ////    {
    ////        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"I can't repeat this track for {loops} times!"));
    ////        return;
    ////    }

    ////    var currentTrack = player.Track;
    ////    var artwork = await currentTrack.FetchArtworkAsync();
    ////    var cycledTracks = Enumerable.Repeat(currentTrack, loops);

    ////    await RespondAsync(embed: await _embedHandler.CreateMediaEmbedAsync($"Cycled for {loops} times:", artwork, currentTrack.Title, currentTrack.Url));

    ////    player.Vueue.Enqueue(cycledTracks);
    ////}

    //private async Task<bool> IsPlayerPlaying(LavaPlayer<LavaTrack> player)
    //{
    //    if (player.PlayerState != PlayerState.Playing)
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("Woaaah there, I'm not playing any tracks."));
    //        return false;
    //    }

    //    return true;
    //}

    //private async Task<LavaPlayer<LavaTrack>> TryGetLavaPlayer()
    //{
    //    if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("I'm not connected to a voice channel."));
    //        return null;
    //    }

    //    return player;
    //}
}
