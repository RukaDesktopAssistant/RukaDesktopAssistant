using Microsoft.Win32;

namespace RukaDesktopAssistant.Services;

public sealed class StartupService
{
    private const string RunKey = @"Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    public void SetEnabled(bool enabled, string executablePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true) ?? Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled) key.SetValue("RukaDesktopAssistant", $"\"{executablePath}\"");
        else key.DeleteValue("RukaDesktopAssistant", false);
    }
}
