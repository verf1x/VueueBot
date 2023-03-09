namespace DiscordBot.Core.Handlers;

using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EmbedHandler
{
    private const UInt32 SUCCESS_COLOR = 0x1CB012;
    private const UInt32 ERROR_COLOR = 0xF70202;
    private const UInt32 MEDIA_COLOR = 0xF70202;
    private const UInt32 WARNING_COLOR = 0xF79902;
    private const UInt32 DEFAULT_MESSAGE_COLOR = 0xFF94E8;

    public async Task<Embed> CreateWarningEmbedAsync(string description) 
        => await Task.Run(() 
            => new EmbedBuilder()
                 .WithTitle("Oops...")
                 .WithDescription(description)
                 .WithColor(WARNING_COLOR)
                 .WithCurrentTimestamp()
                 .WithCurrentTimestamp().Build());

    public async Task<Embed> CreateSuccessJoinEmbedAsync(string voiceChannelName, int voicechannelBitrate)
        => await Task.Run(() 
            => new EmbedBuilder()
                .WithTitle("Joined to voice channel:")
                .WithDescription(voiceChannelName)
                .WithColor(SUCCESS_COLOR)
                .AddField($"Bitrate:", voicechannelBitrate / 1000, false)
                .WithCurrentTimestamp()
                .Build());

    public async Task<Embed> CreateExceptionEmbedAsync(string? source, string message)
        => await Task.Run(()
            => new EmbedBuilder()
                .WithTitle("Fatal error:")
                .WithDescription(message)
                .WithColor(ERROR_COLOR)
                .AddField($"From source:", source, false)
                .WithCurrentTimestamp()
                .Build());

    public async Task<Embed> CreateSuccessEmbedAsync(string successMessage)
        => await Task.Run(() 
            => new EmbedBuilder()
                .WithTitle("Success!")
                .WithDescription(successMessage)
                .WithColor(SUCCESS_COLOR)
                .WithCurrentTimestamp()
                .Build());

    public async Task<Embed> CreateMediaEmbedAsync(string title, string artwork, string trackTitle, string trackUrl)
        => await Task.Run(() 
            => new EmbedBuilder()
                .WithTitle(title)
                .WithColor(MEDIA_COLOR)
                .WithImageUrl(artwork)
                .AddField(trackTitle, trackUrl, true)
                .WithFooter("YouTube")
                .WithCurrentTimestamp()
                .Build());

    //public async Task<Embed> CreateLyricsEmbedAsync(StringBuilder lyrics)
    //    => await Task.Run(()
    //        => new EmbedBuilder()        
}
