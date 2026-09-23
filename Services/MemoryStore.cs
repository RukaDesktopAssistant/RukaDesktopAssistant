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
        if (string.IsNullOrWhiteSpace(text)) return;
        if (_memories.Contains(text, StringComparer.Ordinal)) return;
        _memories.Add(text.Trim()); Save();
    }
    public void Forget(string text)
    {
        _memories.RemoveAll(x => x.Equals(text.Trim(), StringComparison.Ordinal)); Save();
    }
    private void Load()
    {
        try { if (File.Exists(_file)) { var data=JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_file)); if(data is not null)_memories.AddRange(data); } } catch { }
    }
    private void Save()
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(_file)!); File.WriteAllText(_file, JsonSerializer.Serialize(_memories, new JsonSerializerOptions { WriteIndented=true })); } catch { }
    }
}
