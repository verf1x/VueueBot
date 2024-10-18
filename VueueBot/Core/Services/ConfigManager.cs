namespace VueueBot.Core.Managers;

using Newtonsoft.Json;

public static class ConfigManager
{
    private static readonly string _configFolder = @"Resources";
    private static readonly string _configFile = @"appsettings.json";
    private static readonly string _configPath = _configFolder + "/" + _configFile;

    public static BotConfig Config { get; private set; }

    static ConfigManager()
    {
        if (!Directory.Exists(_configFolder))
            Directory.CreateDirectory(_configFolder);

        if (!File.Exists(_configPath))
        {
            Config = new BotConfig();
            var json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(_configPath, json);
        }
        else
        {
            var json = File.ReadAllText(_configPath);
            Config = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
}
