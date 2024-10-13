namespace VueueBot.Core.Modules;

public class SettingsModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly Dictionary<string, string> _languageNames = new()
    {
        { "english", "eng" },
        { "russian", "rus" }
    };
}
