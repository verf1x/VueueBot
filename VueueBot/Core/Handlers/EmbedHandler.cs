namespace VueueBot.Core.Handlers;

using System.Text;
using System.Threading.Tasks;

public class EmbedHandler
{
    private const uint SuccessColor = 0x1CB012;
    private const uint ErrorColor = 0xF70202;
    private const uint MediaColor = 0xF70202;
    private const uint WarningColor = 0xF79902;
    private const uint DefaultMessageColor = 0xFF94E8;
    private const uint LyricsColor = 0xA200ff;

    public async Task<Embed> CreateWarningEmbedAsync(string description)
        => await Task.Run(()
            => new EmbedBuilder()
                 .WithTitle("Oops...")
                 .WithDescription(description)
                 .WithColor(WarningColor)
                 .WithCurrentTimestamp()
                 .WithCurrentTimestamp().Build());

    public async Task<Embed> CreateSuccessJoinEmbedAsync(string voiceChannelName, int voicechannelBitrate)
        => await Task.Run(()
            => new EmbedBuilder()
                .WithTitle("Joined to voice channel:")
                .WithDescription(voiceChannelName)
                .WithColor(SuccessColor)
                .AddField($"Bitrate:", voicechannelBitrate / 1000, false)
                .WithCurrentTimestamp()
                .Build());

    public async Task<Embed> CreateExceptionEmbedAsync(Exception ex)
        => await Task.Run(()
            => new EmbedBuilder()
                .WithTitle("Fatal error:")
                .WithDescription(ex.ToString())
                .WithColor(ErrorColor)
                .AddField($"From source:", ex.Source, false)
                .WithCurrentTimestamp()
                .Build());

    public async Task<Embed> CreateSuccessEmbedAsync(string successMessage)
        => await Task.Run(()
            => new EmbedBuilder()
                .WithTitle("Success!")
                .WithDescription(successMessage)
                .WithColor(SuccessColor)
                .WithCurrentTimestamp()
                .Build());

    public async Task<Embed> CreateMediaEmbedAsync(string title, string trackTitle, string trackUrl)
        => await Task.Run(()
            => new EmbedBuilder()
                .WithTitle(title)
                .WithColor(MediaColor)
                //.WithImageUrl(artwork)
                .AddField(trackTitle, trackUrl, true)
                .WithFooter("YouTube")
                .WithCurrentTimestamp()
                .Build());

    //public async Task<Embed> CreateHelpEmbedAsync<T>(string moduleName, IReadOnlyCollection<T> slashCommands)
    //    where T : CommandInfo<T> 
    //{

    //}

    public async Task<Embed> CreateLyricsEmbedAsync(StringBuilder lyrics, string trackName)
        => await Task.Run(()
            => new EmbedBuilder()
                .AddField($"{trackName} lyrics:", lyrics)
                .WithColor(LyricsColor)
                .WithCurrentTimestamp()
                .Build());
}
