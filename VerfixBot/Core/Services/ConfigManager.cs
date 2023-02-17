namespace VerfixMusic.Core.Managers;

using Newtonsoft.Json;

public static class ConfigManager
{
    private static string ConfigFolder = @"Resources";
    private static string ConfigFile = @"appsettings.json";
    private static string ConfigPath = ConfigFolder + "/" + ConfigFile;

    public static BotConfig Config { get; private set; }

    static ConfigManager()
    {
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
        }
        if (!File.Exists(ConfigPath))
        {
            Config = new BotConfig();
            var json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(ConfigPath, json);
        }
        else
        {
            var json = File.ReadAllText(ConfigPath);
            Config = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
}
