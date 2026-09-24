using System.Text.Json;
using RukaDesktopAssistant.Models;

namespace RukaDesktopAssistant.Services;

public sealed class MonitorProfileStore
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "monitors.json");

    private readonly Dictionary<string, MonitorProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);

    public MonitorProfile Get(string id) => _profiles.TryGetValue(id, out var p)
        ? p
        : new MonitorProfile(id);

    public void Set(MonitorProfile profile)
    {
        _profiles[profile.MonitorId] = profile with { Scale = Math.Clamp(profile.Scale, 0.5, 2.0) };
        Save();
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<List<MonitorProfile>>(File.ReadAllText(_file));
            if (data is null) return;
            foreach (var item in data) _profiles[item.MonitorId] = item;
        }
        catch { }
    }

    public MonitorProfileStore() => Load();

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(_profiles.Values.ToList(), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}