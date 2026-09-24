using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class ProviderSettings
{
    private readonly string _file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RukaDesktopAssistant", "ai.json");

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
            ApiKey = Unprotect(data.ApiKey ?? "");
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
            var copy = new ProviderSettings
            {
                Provider = Provider,
                Endpoint = Endpoint,
                ApiKey = Protect(ApiKey ?? ""),
                Model = Model,
                SendRecentHistory = SendRecentHistory,
                HistoryCount = HistoryCount
            };
            File.WriteAllText(_file, JsonSerializer.Serialize(copy, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    public string EffectiveApiKey => string.IsNullOrWhiteSpace(ApiKey) ? Environment.GetEnvironmentVariable("RUKA_AI_API_KEY") ?? "" : ApiKey;

    private static string Protect(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        try
        {
            var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
            return "dpapi:" + Convert.ToBase64String(bytes);
        }
        catch { return value; }
    }

    private static string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith("dpapi:", StringComparison.Ordinal)) return value;
        try
        {
            var bytes = ProtectedData.Unprotect(Convert.FromBase64String(value[6..]), null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch { return ""; }
    }
}