namespace VerfixMusic.Core.Managers;

using Newtonsoft.Json;

public struct BotConfig
{
    [JsonProperty("token")]
    public string Token { get; private set; }

    [JsonProperty("prefix")]
    public string Prefix { get; private set; }
}
