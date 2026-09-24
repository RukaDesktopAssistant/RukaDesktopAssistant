using System.Text.Json;
using RukaDesktopAssistant.Models;

namespace RukaDesktopAssistant.Services;

public sealed class PersonalityStore
{
    private readonly string _file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RukaDesktopAssistant", "personalities.json");
    private readonly List<PersonalityProfile> _profiles = [];
    public IReadOnlyList<PersonalityProfile> Profiles => _profiles;

    public PersonalityStore() => Load();

    public void Add(PersonalityProfile profile)
    {
        _profiles.RemoveAll(x => x.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase));
        _profiles.Add(profile);
        Save();
    }

    public void Remove(string name)
    {
        _profiles.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        Save();
    }

    public void Clear()
    {
        _profiles.Clear();
        Save();
    }

    private void Load()
    {
        try {
            if (!File.Exists(_file)) return;
            var x = JsonSerializer.Deserialize<List<PersonalityProfile>>(File.ReadAllText(_file));
            if (x is not null) _profiles.AddRange(x);
        } catch { }
    }

    private void Save()
    {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true }));
        } catch { }
    }
}
