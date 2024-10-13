namespace VueueBot.Core.Managers;

using Newtonsoft.Json;

public struct BotConfig
{
    [JsonProperty("token")]
    public string Token { get; private set; }

    [JsonProperty("guild_id")]
    public ulong TestGuildId { get; private set;  }
}
