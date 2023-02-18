namespace VerfixMusic.Common;

using Discord;

public class VerfixEmbedBuilder : EmbedBuilder
{
    public VerfixEmbedBuilder()
    {
        WithColor(new Color(0xbcf542));
        WithFooter($"{DateTime.UtcNow} UTC +00:00");
    }
}
