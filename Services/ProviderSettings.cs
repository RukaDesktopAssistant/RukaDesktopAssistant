using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class ProviderSettings
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "ai.json");

    public string Provider { get; set; } = "local";
    public string Endpoint { get; set; } = "";
    public bool SendRecentHistory { get; set; } = true;
    public int HistoryCount { get; set; } = 20;

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<ProviderSettings>(File.ReadAllText(_file));
            if (data is null) return;
            Provider = string.IsNullOrWhiteSpace(data.Provider) ? "local" : data.Provider;
            Endpoint = data.Endpoint ?? "";
            SendRecentHistory = data.SendRecentHistory;
            HistoryCount = Math.Clamp(data.HistoryCount, 1, 100);
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
