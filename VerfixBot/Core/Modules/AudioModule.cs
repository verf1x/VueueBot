namespace VerfixMusic.Core.Commands;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Core.Handlers;
using System;
using VerfixMusic.Core.Managers;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

public class MusicModule : InteractionModuleBase<ShardedInteractionContext>
{
    #region Fields
    private readonly LavaNode _lavaNode;
    private readonly AudioService _audioService;
    private InteractionHandler? _handler;
    private readonly EmbedHandler _embedHandler;
    private static readonly IEnumerable<int> _range = Enumerable.Range(1900, 2000);
    #endregion

    #region Properties
    public InteractionService? Commands { get; set; }
    #endregion

    #region Ctor
    public MusicModule(LavaNode lavaNode, AudioService audioService, InteractionHandler handler, EmbedHandler embedHandler)
    {
        _lavaNode = lavaNode;
        _audioService = audioService;
        _handler = handler;
        _embedHandler = embedHandler;
    }
    #endregion

    [SlashCommand("join", "Joins to the voice channel")]
    public async Task JoinAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild))
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"I'm already connected to a voice channel!"));
            return;
        }

        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel is null)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"You must be connected to a voice channel!"));
            return;
        }

        try
        {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await RespondAsync(embed: await _embedHandler.CreateSuccessJoinEmbedAsync(voiceState.VoiceChannel.Name,
                                                                                      voiceState.VoiceChannel.Bitrate));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    [SlashCommand("leave", "Leaves from voice channel", runMode: RunMode.Async)]
    public async Task LeaveAsync()
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        var voiceChannel = ((IVoiceState)Context.User).VoiceChannel ?? player.VoiceChannel;
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
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    [SlashCommand("play", "Plays media from YouTube")]
    public async Task PlayAsync(string searchQuery)
    {

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("Please provide search terms."));
            return;
        }

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel is null)
            {
                await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("You must be connected to a voice channel!"));
                return;
            }

            try
            {
                player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);

            }
            catch (Exception exception)
            {
                await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
            }
        }

        var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, searchQuery);
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"I wasn't able to find anything for `{searchQuery}`."));
            return;
        }

        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
        {
            player.Vueue.Enqueue(searchResponse.Tracks);
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Enqueued {searchResponse.Tracks.Count} songs."));
        }
        else
        {
            var track = searchResponse.Tracks.FirstOrDefault();
            player.Vueue.Enqueue(track);

            var artwork = await track.FetchArtworkAsync();

            await RespondAsync(embed: await _embedHandler.CreateMediaEmbedAsync("Added to playlist:", artwork, track.Title, track.Url));
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
        {
            return;
        }

        player.Vueue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(lavaTrack);
        await player.SetVolumeAsync(30);
    }

    [SlashCommand("pause", "Pauses current track", runMode: RunMode.Async)]
    public async Task PauseAsync()
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        if (player.PlayerState is not PlayerState.Playing)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("I cannot pause when I'm not playing anything!"));
            return;
        }

        try
        {
            await player.PauseAsync();
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Paused: {player.Track.Title}"));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    [SlashCommand("resume", "Resumes paused track", runMode: RunMode.Async)]
    public async Task ResumeAsync()
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        if (player.PlayerState is not PlayerState.Paused)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("I cannot resume when I'm not playing anything!"));
            return;
        }

        try
        {
            await player.ResumeAsync();
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Resumed: {player.Track.Title}"));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    [SlashCommand("stop", "Stops playing media", runMode: RunMode.Async)]
    public async Task StopAsync()
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        if (!await IsPlayerPlaying(player))
        {
            return;
        }

        try
        {
            await player.StopAsync();
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync("No longer playing anything."));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    [SlashCommand("skip", "Skip current track if more than 50% votes to skip this track", runMode: RunMode.Async)]
    public async Task SkipAsync()
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        if (!await IsPlayerPlaying(player))
        {
            return;
        }

        var voiceChannelUsers = ((SocketGuild)player.VoiceChannel.Guild).Users
            .Where(x => !x.IsBot)
            .ToArray();
        if (_audioService.VoteQueue.Contains(Context.User.Id))
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("You can't vote again."));
            return;
        }

        _audioService.VoteQueue.Add(Context.User.Id);
        var percentage = _audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
        if (percentage <= 50)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("You need more than 50% votes to skip this song."));
            return;
        }

        try
        {
            var (skipped, currenTrack) = await player.SkipAsync();
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"Skipped: {skipped.Title}"));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    //[SlashCommand("seek", "VerfixMusic seek timeStamp", runMode: RunMode.Async)]
    //public async Task SeekAsync(TimeSpan timeSpan)
    //{
    //    var embedBuilder = new CustomEmbedBuilder();

    //    var player = await TryGetLavaPlayer(embedBuilder);
    //    if (player == null)
    //    {
    //        return;
    //    }

    //    if (!await IsPlayerPlaying(embedBuilder, player))
    //    {
    //        return;
    //    }

    //    try
    //    {
    //        embedBuilder.Title = $"I've seeked `{player.Track.Title}` to {timeSpan}.";

    //        await player.SeekAsync(timeSpan);
    //        await RespondAsync(embed: embedBuilder.Build());
    //    }
    //    catch (Exception exception)
    //    {
    //        await CallException(embedBuilder, exception);
    //    }
    //}

    [SlashCommand("volume", "Change media player volume", runMode: RunMode.Async)]
    public async Task SetVolumeAsync(ushort volume)
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        try
        {
            await player.SetVolumeAsync(volume);
            await RespondAsync(embed: await _embedHandler.CreateSuccessEmbedAsync($"I've changed the player volume to {volume}."));
        }
        catch (Exception exception)
        {
            await RespondAsync(embed: await _embedHandler.CreateExceptionEmbedAsync(exception.Source, exception.Message));
        }
    }

    [SlashCommand("nowplaying", "Shows what is currently playing")]
    public async Task NowPlayingAsync()
    {
        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        if (!await IsPlayerPlaying(player))
        {
            return;
        }

        var track = player.Track;
        var artwork = await track.FetchArtworkAsync();

        await RespondAsync(embed: await _embedHandler.CreateMediaEmbedAsync($"Now Playing:", artwork, track.Title, track.Url));
    }

    //[SlashCommand("genius", "Returns genius lyrics for current song")]
    //public async Task ShowGeniusLyrics()
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

    //    var lyrics = await LyricsResolver.SearchGeniusAsync(player.Track);
    //    if (string.IsNullOrWhiteSpace(lyrics))
    //    {
    //        await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"No lyrics found for {player.Track.Title}"));
    //        return;
    //    }

    //    var splitLyrics = lyrics.Split(Environment.NewLine);
    //    var stringBuilder = new StringBuilder();
    //    foreach (var line in splitLyrics)
    //    {
    //        if (_range.Contains(stringBuilder.Length))
    //        {
    //            await RespondAsync($"```{stringBuilder}```");
    //            stringBuilder.Clear();
    //        }
    //        else
    //        {
    //            stringBuilder.AppendLine(line.TrimEnd('\n'));
    //        }
    //    }

    //    embedBuilder.Title = $"Genius Lyrics";

    //    await RespondAsync($"```{stringBuilder}```");
    //}

    [SlashCommand("loop", "Loops track for count times", runMode: RunMode.Async)]
    public async Task LoopCurrentTrack(int count)
    {
        count--;

        var player = await TryGetLavaPlayer();
        if (player is null)
        {
            return;
        }

        if (!await IsPlayerPlaying(player))
        {
            return;
        }

        if (count <= 0)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync($"I can't repeat this track for {count} times!"));
            return;
        }

        var currentTrack = player.Track;
        var artwork = await currentTrack.FetchArtworkAsync();
        var cycledTracks = Enumerable.Repeat(currentTrack, count);

        await RespondAsync(embed: await _embedHandler.CreateMediaEmbedAsync($"Cycled for {count} times:", artwork, currentTrack.Title, currentTrack.Url));

        player.Vueue.Enqueue(cycledTracks);
    }

    private async Task<bool> IsPlayerPlaying(LavaPlayer<LavaTrack> player)
    {
        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("Woaaah there, I'm not playing any tracks."));
            return false;
        }

        return true;
    }

    private async Task<LavaPlayer<LavaTrack>?> TryGetLavaPlayer()
    {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync(embed: await _embedHandler.CreateWarningEmbedAsync("I'm not connected to a voice channel."));
            return null;
        }

        return player;
    }
}
