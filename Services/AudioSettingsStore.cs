using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class AudioSettingsStore
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "audio.json");

    public AudioSettings Current { get; } = new();

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<AudioSettings>(File.ReadAllText(_file));
            if (data is null) return;
            Current.WakeWordEnabled = data.WakeWordEnabled;
            Current.WakePhrase = string.IsNullOrWhiteSpace(data.WakePhrase) ? "ねぇ、るか" : data.WakePhrase;
            Current.TtsEnabled = data.TtsEnabled;
            Current.TtsRate = Math.Clamp(data.TtsRate, -10, 10);
            Current.TtsVolume = Math.Clamp(data.TtsVolume, 0, 100);
        }
        catch { }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
