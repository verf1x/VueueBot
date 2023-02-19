namespace VerfixMusic.Core.Commands;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("Leave", RunMode = RunMode.Async)]
    public async Task LeaveAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to any voice channels!";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    //[Command("Play")]
    //public async Task PlayAsync([Remainder] string searchQuery)
    //{
    //    var embedBuilder = new VerfixEmbedBuilder();

    //    if (string.IsNullOrWhiteSpace(searchQuery))
    //    {
    //        await ReplyAsync("Please provide search terms.");
    //        return;
    //    }

    //    if (!_lavaNode.HasPlayer(Context.Guild))
    //    {
    //        await ReplyAsync("I'm not connected to a voice channel.");
    //        return;
    //    }

    //    var queries = searchQuery.Split(' ');
    //    foreach (var query in queries)
    //    {
    //        var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, searchQuery);
    //        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
    //        {
    //            embedBuilder.Color = Color.Red;
    //            embedBuilder.Title = $"I wasn't able to find anything for `{searchQuery}`.";

    //            await ReplyAsync(embed: embedBuilder.Build());
    //            return;
    //        }

    //        _lavaNode.TryGetPlayer(Context.Guild, out var player);

    //        if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
    //        {
    //            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
    //            {
    //                foreach (var track in searchResponse.Tracks)
    //                {
    //                    player.Vueue.Enqueue(track);
    //                }

    //                await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
    //            }
    //            else
    //            {
    //                var track = searchResponse.Tracks.FirstOrDefault();
    //                player.Vueue.Enqueue(track);
    //                await ReplyAsync($"Enqueued: {track?.Title}");
    //            }
    //        }
    //        else
    //        {
    //            var track = searchResponse.Tracks.FirstOrDefault();

    //            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
    //            {
    //                embedBuilder.Title = $"Enqueued {searchResponse.Tracks.Count} songs.";

    //                player.Vueue.Enqueue(searchResponse.Tracks);
    //                await ReplyAsync(embed: embedBuilder.Build());
    //            }
    //            else
    //            {
    //                await player.PlayAsync(track);
    //                await player.SetVolumeAsync(40);
    //                await ReplyAsync($"Now Playing: {track?.Title}");
    //            }
    //        }
    //    }
    //}

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
                embedBuilder.Color = Color.Red;
                embedBuilder.Title = "Error!";
                embedBuilder.AddField("Exception thrown", exception.Message, true);

                await ReplyAsync(embed: embedBuilder.Build());
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

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("Resume", RunMode = RunMode.Async)]
    public async Task ResumeAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("Stop", RunMode = RunMode.Async)]
    public async Task StopAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (player.PlayerState == PlayerState.Stopped)
        {
            embedBuilder.Title = "Woaaah there, I can't stop the stopped forced.";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("Skip", RunMode = RunMode.Async)]
    public async Task SkipAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            embedBuilder.Title = "Woaaah there, I can't skip when nothing is playing.";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("Seek", RunMode = RunMode.Async)]
    public async Task SeekAsync(TimeSpan timeSpan)
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            embedBuilder.Title = "Woaaah there, I can't seek when nothing is playing.";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("Volume", RunMode = RunMode.Async)]
    public async Task VolumeAsync(ushort volume)
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
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
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error!";
            embedBuilder.AddField("Exception thrown", exception.Message, true);

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }

    [Command("NowPlaying"), Alias("Np")]
    public async Task NowPlayingAsync()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            embedBuilder.Title = "Woaaah there, I'm not playing any tracks.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        var track = player.Track;
        var artwork = await track.FetchArtworkAsync();

        var embed = new VerfixEmbedBuilder()
            .WithAuthor(track.Author, Context.Client.CurrentUser.GetAvatarUrl(), track.Url)
            .WithTitle($"Now Playing: {track.Title}")
            .WithImageUrl(artwork)
            .WithFooter($"{track.Position}/{track.Duration}");

        await ReplyAsync(embed: embed.Build());
    }

    [Command("Genius", RunMode = RunMode.Async)]
    public async Task ShowGeniusLyrics()
    {
        var embedBuilder = new VerfixEmbedBuilder();

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            embedBuilder.Title = "I'm not connected to a voice channel.";

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            embedBuilder.Title = "Woaaah there, I'm not playing any tracks.";

            await ReplyAsync(embed: embedBuilder.Build());
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

            //embedBuilder.AddField("", line, true);
        }

        embedBuilder.Title = $"Genius Lyrics";

        //await ReplyAsync(embed: embedBuilder.Build());
        await ReplyAsync($"```{stringBuilder}```");
    }

    //[Command("OVH", RunMode = RunMode.Async)]
    //public async Task ShowOvhLyrics()
    //{
    //    if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
    //    {
    //        await ReplyAsync("I'm not connected to a voice channel.");
    //        return;
    //    }

    //    if (player.PlayerState != PlayerState.Playing)
    //    {
    //        await ReplyAsync("Woaaah there, I'm not playing any tracks.");
    //        return;
    //    }

    //    var lyrics = await LyricsResolver.SearchOvhAsync(player.Track);
    //    if (string.IsNullOrWhiteSpace(lyrics))
    //    {
    //        await ReplyAsync($"No lyrics found for {player.Track.Title}");
    //        return;
    //    }

    //    var splitLyrics = lyrics.Split(Environment.NewLine);
    //    var stringBuilder = new StringBuilder();
    //    foreach (var line in splitLyrics)
    //    {
    //        if (_range.Contains(stringBuilder.Length))
    //        {
    //            await ReplyAsync($"```{stringBuilder}```");
    //            stringBuilder.Clear();
    //        }
    //        else
    //        {
    //            stringBuilder.AppendLine(line);
    //        }
    //    }

    //    await ReplyAsync($"```{stringBuilder}```");
    //}
}
