using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed class MemoryStore
{
    private readonly string _file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RukaDesktopAssistant", "memory.json");
    private readonly List<string> _memories = [];
    public IReadOnlyList<string> Memories => _memories;

    public MemoryStore() => Load();

    public void Remember(string text)
    {
        var value = text.Trim();
        if (value.Length == 0) return;
        if (!_memories.Contains(value, StringComparer.OrdinalIgnoreCase)) _memories.Add(value);
        Save();
    }

    public void Forget(string text)
    {
        _memories.RemoveAll(x => x.Contains(text.Trim(), StringComparison.OrdinalIgnoreCase));
        Save();
    }

    public void Clear()
    {
        _memories.Clear();
        Save();
    }

    private void Load()
    {
        try {
            if (!File.Exists(_file)) return;
            var x = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_file));
            if (x is not null) _memories.AddRange(x.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase));
        } catch { }
    }

    private void Save()
    {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(_memories, new JsonSerializerOptions { WriteIndented = true }));
        } catch { }
    }
}
