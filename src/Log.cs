using System;
using Godot;

namespace RestSiteUpgradeAll;

internal static class Log
{
    private const string Prefix = "[RestSiteUpgradeAll]";

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message, Exception? exception = null)
    {
        var fullMessage = exception is null
            ? message
            : $"{message}\n{exception}";

        Write("ERROR", fullMessage);
    }

    private static void Write(string level, string message)
    {
        var line = $"{Prefix} [{level}] {message}";

        try
        {
            GD.Print(line);
        }
        catch
        {
            Console.WriteLine(line);
        }
    }
}
