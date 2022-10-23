using System;
using System.IO;

namespace Wobble.Services;

public static class LoggingService
{
    private const string CHAT_LOG_DIRECTORY = "./ChatLogs";

    static LoggingService()
    {
        if (!Directory.Exists(CHAT_LOG_DIRECTORY))
        {
            Directory.CreateDirectory(CHAT_LOG_DIRECTORY);
        }
    }

    public static void LogMessage(string message)
    {
        File.AppendAllText(
            Path.Combine(CHAT_LOG_DIRECTORY, $"Wobble-{DateTime.Now:yyyy-MM-dd}.log"),
            $"{message}{Environment.NewLine}");
    }
}