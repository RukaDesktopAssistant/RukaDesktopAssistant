using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class PermissionManager
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "permissions.json");

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

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var loaded = JsonSerializer.Deserialize<PermissionManager>(File.ReadAllText(_file));
            if (loaded is null) return;
            AllowScreenRead = loaded.AllowScreenRead;
            AllowLaunchApps = loaded.AllowLaunchApps;
            AllowCloseApps = loaded.AllowCloseApps;
            AllowFileRead = loaded.AllowFileRead;
            AllowFileWrite = loaded.AllowFileWrite;
            AllowBrowserControl = loaded.AllowBrowserControl;
            AllowSystemSettings = loaded.AllowSystemSettings;
            RequireConfirmationForDangerousActions = loaded.RequireConfirmationForDangerousActions;
        }
        catch { }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
