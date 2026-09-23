using System.Diagnostics;

namespace RukaDesktopAssistant.Services;

public sealed record AppContextInfo(string? ProcessName, string? WindowTitle, bool IsKnownGame);

public static class AppContextService
{
    private static readonly HashSet<string> KnownGames = new(StringComparer.OrdinalIgnoreCase)
    {
        "RobloxPlayerBeta", "FortniteClient-Win64-Shipping", "VALORANT-Win64-Shipping",
        "r5apex", "ApexLegends", "Overwatch", "Minecraft"
    };

    public static AppContextInfo GetForegroundContext()
    {
        // Safe first version: process/window metadata only. No screen contents are captured.
        var process = Process.GetProcesses()
            .Where(p => !p.HasExited)
            .Select(p =>
            {
                try { return (p, title: p.MainWindowTitle); }
                catch { return (p, title: ""); }
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.title))
            .OrderByDescending(x => x.p.Responding)
            .FirstOrDefault();

        if (process.p is null) return new AppContextInfo(null, null, false);
        return new AppContextInfo(process.p.ProcessName, process.title, KnownGames.Contains(process.p.ProcessName));
    }
}
