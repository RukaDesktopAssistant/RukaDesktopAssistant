using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class ProviderSettings
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "ai.json");

    public string Provider { get; set; } = "local";
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gpt-4o-mini";
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
            Endpoint = string.IsNullOrWhiteSpace(data.Endpoint) ? "https://api.openai.com/v1/chat/completions" : data.Endpoint;
            ApiKey = data.ApiKey ?? "";
            Model = string.IsNullOrWhiteSpace(data.Model) ? "gpt-4o-mini" : data.Model;
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

    public string EffectiveApiKey =>
        string.IsNullOrWhiteSpace(ApiKey)
            ? Environment.GetEnvironmentVariable("RUKA_AI_API_KEY") ?? ""
            : ApiKey;
}
