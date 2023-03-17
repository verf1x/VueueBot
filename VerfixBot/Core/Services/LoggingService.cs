namespace DiscordBot.Core.Services;

public class LoggingService
{
    public async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
    {
        if (severity.Equals(null))
        {
            severity = LogSeverity.Warning;
        }
        await AppendAsync($"{GetSeverityString(severity)}", GetConsoleColor(severity));
        await AppendAsync($" [{SourceToString(src)}] ", ConsoleColor.DarkGray);

        if (!string.IsNullOrWhiteSpace(message))
        {
            await AppendAsync($"{message}\n", ConsoleColor.White);
        }
        else if (exception == null)
        {
            await AppendAsync("Uknown Exception. Exception Returned Null.\n", ConsoleColor.DarkRed);
        }
        else if (exception.Message == null)
        {
            await AppendAsync($"Unknownk \n{exception.StackTrace}\n", GetConsoleColor(severity));
        }
        else
        {
            await AppendAsync($"{exception.Message ?? "Unknownk"}\n{exception.StackTrace ?? "Unknown"}\n", GetConsoleColor(severity));
        }
    }

    public async Task LogCriticalAsync(string source, string message, Exception exc = null)
    {
        await LogAsync(source, LogSeverity.Critical, message, exc);
    }

    public async Task LogInformationAsync(string source, string message)
    {
        await LogAsync(source, LogSeverity.Info, message);
    }

    private async Task AppendAsync(string message, ConsoleColor color)
    {
        await Task.Run(() =>
               {
                   Console.ForegroundColor = color;
                   Console.Write(message);
               });
    }

    private string SourceToString(string src)
    {
        return src.ToLower() switch
        {
            "discord"           => "DISCD",
            "victoria"          => "VICTR",
            "audio"             => "AUDIO",
            "admin"             => "ADMIN",
            "gateway"           => "GTWAY",
            "blacklist"         => "BLAKL",
            "lavanode_0_socket" => "LAVAS",
            "lavanode_0"        => "LAVA#",
            "rest"              => "REST",
            "bot"               => "BOTWN",
            _ => src,
        };
    }

    private string GetSeverityString(LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Critical => "CRIT",
            LogSeverity.Debug    => "DBUG",
            LogSeverity.Error    => "EROR",
            LogSeverity.Info     => "INFO",
            LogSeverity.Verbose  => "VERB",
            LogSeverity.Warning  => "WARN",
            _ => "UNKN",
        };
    }

    private ConsoleColor GetConsoleColor(LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Critical => ConsoleColor.Red,
            LogSeverity.Debug    => ConsoleColor.Magenta,
            LogSeverity.Error    => ConsoleColor.DarkRed,
            LogSeverity.Info     => ConsoleColor.Green,
            LogSeverity.Verbose  => ConsoleColor.DarkCyan,
            LogSeverity.Warning  => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
    }
}
