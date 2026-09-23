using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class AppearanceSettings
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "appearance.json");

    public double Scale { get; set; } = 1.0;
    public double Opacity { get; set; } = 1.0;
    public string Position { get; set; } = "bottom-right";
    public bool AlwaysOnTop { get; set; } = true;
    public bool ShowSpeechBubble { get; set; } = true;

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<AppearanceSettings>(File.ReadAllText(_file));
            if (data is null) return;
            Scale = Math.Clamp(data.Scale, 0.5, 2.0);
            Opacity = Math.Clamp(data.Opacity, 0.2, 1.0);
            Position = string.IsNullOrWhiteSpace(data.Position) ? "bottom-right" : data.Position;
            AlwaysOnTop = data.AlwaysOnTop;
            ShowSpeechBubble = data.ShowSpeechBubble;
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
