namespace RukaDesktopAssistant.Services;

public sealed class PermissionManager
{
    public bool AllowScreenRead { get; set; }
    public bool AllowLaunchApps { get; set; }
    public bool AllowCloseApps { get; set; }
    public bool AllowFileRead { get; set; }
    public bool AllowFileWrite { get; set; }
    public bool AllowBrowserControl { get; set; }
    public bool AllowSystemSettings { get; set; }
    public bool RequireConfirmationForDangerousActions { get; set; } = true;

    public bool CanPerform(string capability) => capability switch
    {
        "screen.read" => AllowScreenRead,
        "apps.launch" => AllowLaunchApps,
        "apps.close" => AllowCloseApps,
        "files.read" => AllowFileRead,
        "files.write" => AllowFileWrite,
        "browser.control" => AllowBrowserControl,
        "system.settings" => AllowSystemSettings,
        _ => false
    };
}
