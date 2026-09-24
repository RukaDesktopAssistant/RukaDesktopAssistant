using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class SettingsStore
{
    private readonly string _file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RukaDesktopAssistant", "settings.json");

    public bool StartWithWindows { get; set; } = true;
    public bool ShowDuringGames { get; set; } = false;
    public bool DiscordWakeWordOnly { get; set; } = true;
    public bool VoiceInputEnabled { get; set; } = false;
    public string WakePhrase { get; set; } = "ねぇ、るか";
    public double VoiceRate { get; set; } = 0;
    public double VoiceVolume { get; set; } = 100;
    public bool NotificationsEnabled { get; set; } = true;
    public bool AutonomousBehaviorEnabled { get; set; } = true;
    public string ActivePersonalityName { get; set; } = "るか";

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<SettingsStore>(File.ReadAllText(_file));
            if (data is null) return;
            StartWithWindows = data.StartWithWindows;
            ShowDuringGames = data.ShowDuringGames;
            DiscordWakeWordOnly = data.DiscordWakeWordOnly;
            VoiceInputEnabled = data.VoiceInputEnabled;
            WakePhrase = string.IsNullOrWhiteSpace(data.WakePhrase) ? "ねぇ、るか" : data.WakePhrase;
            VoiceRate = Math.Clamp(data.VoiceRate, -10, 10);
            VoiceVolume = Math.Clamp(data.VoiceVolume, 0, 100);
            NotificationsEnabled = data.NotificationsEnabled;
            AutonomousBehaviorEnabled = data.AutonomousBehaviorEnabled;
            ActivePersonalityName = string.IsNullOrWhiteSpace(data.ActivePersonalityName) ? "るか" : data.ActivePersonalityName;
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