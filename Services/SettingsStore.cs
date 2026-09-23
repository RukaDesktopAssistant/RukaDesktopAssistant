using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class SettingsStore
{
    private readonly string _file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RukaDesktopAssistant", "settings.json");
    public bool StartWithWindows { get; set; } = true;
    public bool ShowDuringGames { get; set; } = false;
    public bool DiscordWakeWordOnly { get; set; } = true;

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<SettingsStore>(File.ReadAllText(_file));
            if (data is null) return;
            StartWithWindows=data.StartWithWindows; ShowDuringGames=data.ShowDuringGames; DiscordWakeWordOnly=data.DiscordWakeWordOnly;
        } catch {}
    }

    public void Save()
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(_file)!); File.WriteAllText(_file, JsonSerializer.Serialize(this, new JsonSerializerOptions{WriteIndented=true})); } catch {}
    }
}
