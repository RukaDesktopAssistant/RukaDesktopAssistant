using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RukaDesktopAssistant.Services;

public sealed record AppContextInfo(string? ProcessName, string? WindowTitle, bool IsKnownGame);

public static class AppContextService
{
    private static readonly HashSet<string> KnownGames = new(StringComparer.OrdinalIgnoreCase)
    {
        "RobloxPlayerBeta", "FortniteClient-Win64-Shipping", "VALORANT-Win64-Shipping",
        "r5apex", "ApexLegends", "Overwatch", "Minecraft"
    };

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    public static AppContextInfo GetForegroundContext()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == nint.Zero) return new AppContextInfo(null, null, false);

        var titleBuilder = new StringBuilder(512);
        GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);

        GetWindowThreadProcessId(hwnd, out var pid);
        if (pid == 0) return new AppContextInfo(null, titleBuilder.ToString(), false);

        try
        {
            using var process = Process.GetProcessById((int)pid);
            var name = process.ProcessName;
            return new AppContextInfo(name, titleBuilder.ToString(), KnownGames.Contains(name));
        }
        catch
        {
            return new AppContextInfo(null, titleBuilder.ToString(), false);
        }
    }
}
