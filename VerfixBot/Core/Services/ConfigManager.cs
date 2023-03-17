namespace VerfixMusic.Core.Managers;

using Newtonsoft.Json;

public static class ConfigManager
{
    private static readonly string s_configFolder = @"Resources";
    private static readonly string s_configFile = @"appsettings.json";
    private static readonly string s_configPath = s_configFolder + "/" + s_configFile;

    public static BotConfig Config { get; private set; }

    static ConfigManager()
    {
        if (!Directory.Exists(s_configFolder))
        {
            Directory.CreateDirectory(s_configFolder);
        }
        if (!File.Exists(s_configPath))
        {
            Config = new BotConfig();
            var json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(s_configPath, json);
        }
        else
        {
            var json = File.ReadAllText(s_configPath);
            Config = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
}
