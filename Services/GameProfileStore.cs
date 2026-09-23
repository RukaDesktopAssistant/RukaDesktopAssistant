using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed record GameProfile(
    bool ShowCharacter,
    double Scale,
    string Position,
    bool VoiceEnabled,
    bool MoveToSide = true);

public sealed class GameProfileStore
{
    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RukaDesktopAssistant", "games.json");

    private readonly Dictionary<string, GameProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);

    public GameProfileStore() => Load();

    public GameProfile Get(string game) =>
        _profiles.TryGetValue(game, out var p)
            ? p
            : new(true, 1, "side", false, true);

    public void Set(string game, GameProfile profile)
    {
        _profiles[game] = profile with { Scale = Math.Clamp(profile.Scale, 0.5, 2.0) };
        Save();
    }

    public IReadOnlyDictionary<string, GameProfile> GetAll() =>
        new Dictionary<string, GameProfile>(_profiles, StringComparer.OrdinalIgnoreCase);

    private void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var x = JsonSerializer.Deserialize<Dictionary<string, GameProfile>>(File.ReadAllText(_file));
            if (x is null) return;
            foreach (var p in x)
                _profiles[p.Key] = p.Value;
        }
        catch { }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
            File.WriteAllText(_file, JsonSerializer.Serialize(
                _profiles,
                new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
