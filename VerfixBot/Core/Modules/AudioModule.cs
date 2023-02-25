namespace VerfixMusic.Core.Commands;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Numerics;
using System.Text;
using VerfixMusic.Common;
using VerfixMusic.Core.Managers;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Resolvers;
using Victoria.Responses.Search;

public class MusicModule : ModuleBase<ShardedCommandContext>
{
    private readonly LavaNode _lavaNode;
    private readonly AudioService _audioService;
    private static readonly IEnumerable<int> _range = Enumerable.Range(1900, 2000);

    public MusicModule(LavaNode lavaNode, AudioService audioService)
    {
        _lavaNode = lavaNode;
        _audioService = audioService;
    }

    [Command("Join")]
    public async Task JoinAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (_lavaNode.HasPlayer(Context.Guild))
        {
            embedBuilder.Title = $"I'm already connected to a voice channel!";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            embedBuilder.Title = $"You must be connected to a voice channel!";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        try
        {
            embedBuilder.Title = $"Joined {voiceState.VoiceChannel.Name}!";
            embedBuilder.AddField($"Bitrate", voiceState.VoiceChannel.Bitrate / 1000, true);

            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Leave", RunMode = RunMode.Async)]
    public async Task LeaveAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        var voiceChannel = ((IVoiceState)Context.User).VoiceChannel ?? player.VoiceChannel;
        if (voiceChannel == null)
        {
            embedBuilder.Title = "Not sure which voice channel to disconnect from.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        try
        {
            embedBuilder.Title = $"I've left {voiceChannel.Name}!";

            await _lavaNode.LeaveAsync(voiceChannel);
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Play")]
    public async Task PlayAsync([Remainder] string searchQuery)
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            embedBuilder.Title = "Please provide search terms.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                embedBuilder.Title = "You must be connected to a voice channel!";

                await ReplyAsync(embed: embedBuilder.Build());
                return;
            }

            try
            {
                player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);

                embedBuilder.Title = $"Joined {voiceState.VoiceChannel.Name}!";

                await ReplyAsync(embed: embedBuilder.Build());
            }
            catch (Exception exception)
            {
                await CallException(embedBuilder, exception);
            }
        }

        var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, searchQuery);
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
        {
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = $"I wasn't able to find anything for `{searchQuery}`.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
        {
            embedBuilder.Title = $"Enqueued {searchResponse.Tracks.Count} songs.";

            player.Vueue.Enqueue(searchResponse.Tracks);
            await ReplyAsync(embed: embedBuilder.Build());
        }
        else
        {
            var track = searchResponse.Tracks.FirstOrDefault();
            player.Vueue.Enqueue(track);


            var artwork = await track.FetchArtworkAsync();

            embedBuilder.Title = $"Added to playlist:";
            embedBuilder.WithImageUrl(artwork);
            embedBuilder.AddField($"{track?.Title}", track?.Url, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
        {
            return;
        }

        player.Vueue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(lavaTrack);
        await player.SetVolumeAsync(30);
    }

    [Command("Pause", RunMode = RunMode.Async)]
    public async Task PauseAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            embedBuilder.Title = "I cannot pause when I'm not playing anything!";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        try
        {
            embedBuilder.Title = $"Paused: {player.Track.Title}";

            await player.PauseAsync();
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Resume", RunMode = RunMode.Async)]
    public async Task ResumeAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (player.PlayerState != PlayerState.Paused)
        {
            embedBuilder.Title = "I cannot resume when I'm not playing anything!";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        try
        {
            embedBuilder.Title = $"Resumed: {player.Track.Title}";

            await player.ResumeAsync();
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Stop", RunMode = RunMode.Async)]
    public async Task StopAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (!await IsPlayerPlaying(embedBuilder, player))
        {
            return;
        }

        try
        {
            embedBuilder.Title = "No longer playing anything.";

            await player.StopAsync();
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Skip", RunMode = RunMode.Async)]
    public async Task SkipAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (!await IsPlayerPlaying(embedBuilder, player))
        {
            return;
        }

        var voiceChannelUsers = ((SocketGuild)player.VoiceChannel.Guild).Users
            .Where(x => !x.IsBot)
            .ToArray();
        if (_audioService.VoteQueue.Contains(Context.User.Id))
        {
            embedBuilder.Title = "You can't vote again.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        _audioService.VoteQueue.Add(Context.User.Id);
        var percentage = _audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
        if (percentage < 85)
        {
            embedBuilder.Title = "You need more than 85% votes to skip this song.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        try
        {
            var (skipped, currenTrack) = await player.SkipAsync();

            embedBuilder.Title = $"Skipped: {skipped.Title}\nNow Playing: {currenTrack.Title}";

            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Seek", RunMode = RunMode.Async)]
    public async Task SeekAsync(TimeSpan timeSpan)
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (!await IsPlayerPlaying(embedBuilder, player))
        {
            return;
        }

        try
        {
            embedBuilder.Title = $"I've seeked `{player.Track.Title}` to {timeSpan}.";

            await player.SeekAsync(timeSpan);
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("Volume", RunMode = RunMode.Async)]
    public async Task VolumeAsync(ushort volume)
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        try
        {
            embedBuilder.Title = $"I've changed the player volume to {volume}.";

            await player.SetVolumeAsync(volume);
            await ReplyAsync(embed: embedBuilder.Build());
        }
        catch (Exception exception)
        {
            await CallException(embedBuilder, exception);
        }
    }

    [Command("NowPlaying"), Alias("Np")]
    public async Task NowPlayingAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (!await IsPlayerPlaying(embedBuilder, player))
        {
            return;
        }

        var track = player.Track;
        var artwork = await track.FetchArtworkAsync();
        var embed = new VerfixEmbedBuilder()

            .WithTitle($"Now Playing:")
            .AddField($"{track.Title}", track.Url, true)
            .WithImageUrl(artwork)
            .WithAuthor(track.Author, Context.Client.CurrentUser.GetAvatarUrl(), track.Url);

        await ReplyAsync(embed: embed.Build());
    }

    [Command("Genius", RunMode = RunMode.Async)]
    public async Task ShowGeniusLyrics()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if (!await IsPlayerPlaying(embedBuilder, player))
        {
            return;
        }

        var lyrics = await LyricsResolver.SearchGeniusAsync(player.Track);
        if (string.IsNullOrWhiteSpace(lyrics))
        {
            embedBuilder.Title = $"No lyrics found for {player.Track.Title}";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        var splitLyrics = lyrics.Split(Environment.NewLine);
        var stringBuilder = new StringBuilder();
        foreach (var line in splitLyrics)
        {
            if (_range.Contains(stringBuilder.Length))
            {
                await ReplyAsync($"```{stringBuilder}```");
                stringBuilder.Clear();
            }
            else
            {
                stringBuilder.AppendLine(line.TrimEnd('\n'));
            }
        }

        embedBuilder.Title = $"Genius Lyrics";

        await ReplyAsync($"```{stringBuilder}```");
    }

    [Command("Loop", RunMode = RunMode.Async), RequireOwner]
    public async Task LoopCurrentTrack(int count)
    {
        var embedBuilder = new VerfixEmbedBuilder();

        var player = await TryGetLavaPlayer(embedBuilder);
        if (player == null)
        {
            return;
        }

        if(!await IsPlayerPlaying(embedBuilder, player))
        {
            return;
        }

        if(count <= 0)
        {
            embedBuilder.Title = $"I can't repeat this track for {count} times!";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        var currentTrack = player.Track;

        var artwork = await currentTrack.FetchArtworkAsync();

        embedBuilder.Title = $"Cycled for {count} times:";
        embedBuilder.AddField($"{currentTrack.Title}", currentTrack.Url, true);
        embedBuilder.WithImageUrl(artwork);
        embedBuilder.WithAuthor(currentTrack.Author, Context.Client.CurrentUser.GetAvatarUrl(), currentTrack.Url);

        var cycledTracks = Enumerable.Repeat(currentTrack, count);

        await ReplyAsync(embed: embedBuilder.Build());

        player.Vueue.Enqueue(cycledTracks);
    }

    private async Task CallException(VerfixEmbedBuilder embedBuilder, Exception ex)
    {
        embedBuilder.Color = Color.Red;
        embedBuilder.Title = "Error!";
        embedBuilder.AddField("Exception thrown", ex.Message, true);

        await ReplyAsync(embed: embedBuilder.Build());
    }

    private async Task<bool> IsPlayerPlaying(VerfixEmbedBuilder embedBuilder, LavaPlayer<LavaTrack> player)
    {
        if (player.PlayerState != PlayerState.Playing)
        {
            embedBuilder.Title = "Woaaah there, I'm not playing any tracks.";

            await ReplyAsync(embed: embedBuilder.Build());
            return false;
        }

        return true;
    }

    private async Task<LavaPlayer<LavaTrack>> TryGetLavaPlayer(VerfixEmbedBuilder embedBuilder)
    {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
            return null;
        }

        return player;
    }
}
