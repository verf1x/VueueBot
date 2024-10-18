namespace VueueBot.Core.Services;

public class LoggingService
{
    public async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
    {
        if (severity.Equals(null))
        {
            severity = LogSeverity.Warning;
        }

        await AppendAsync($"{GetSeverityString(severity)}", GetConsoleColorByLogSeverity(severity));
        await AppendAsync($" [{SourceToString(src)}] ", ConsoleColor.DarkGray);

        if (!string.IsNullOrWhiteSpace(message))
        {
            if (!severity.Equals(LogSeverity.Warning))
            {
                await AppendAsync($"{message}\n{exception}", ConsoleColor.White);
            }
            else
            {
                await AppendAsync($"{message}: {exception?.Message}\n", GetConsoleColorByLogSeverity(LogSeverity.Warning));
            }
        }
        else if (exception == null)
        {
            await AppendAsync("Uknown Exception. Exception Returned Null.\n", ConsoleColor.DarkRed);
        }
        else if (exception.Message == null)
        {
            await AppendAsync($"Unknownk \n{exception.StackTrace}\n", GetConsoleColorByLogSeverity(severity));
        }
        else
        {
            await AppendAsync($"{exception.Message ?? "Unknown"}\n{exception.StackTrace ?? "Unknown"}\n", GetConsoleColorByLogSeverity(severity));
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
        => src.ToLower() switch
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

    private string GetSeverityString(LogSeverity severity)
        => severity switch
        {
            LogSeverity.Critical => "CRIT",
            LogSeverity.Debug    => "DBUG",
            LogSeverity.Error    => "EROR",
            LogSeverity.Info     => "INFO",
            LogSeverity.Verbose  => "VERB",
            LogSeverity.Warning  => "WARN",
            _ => "UNKN",
        };

    private ConsoleColor GetConsoleColorByLogSeverity(LogSeverity severity)
        => severity switch
        {
            LogSeverity.Critical => ConsoleColor.Red,
            LogSeverity.Debug    => ConsoleColor.Magenta,
            LogSeverity.Error    => ConsoleColor.DarkRed,
            LogSeverity.Info     => ConsoleColor.Green,
            LogSeverity.Verbose  => ConsoleColor.DarkCyan,
            LogSeverity.Warning  => ConsoleColor.Red,
            _ => ConsoleColor.White,
        };
}
