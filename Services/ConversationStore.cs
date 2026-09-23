using System.Text.Json;

namespace RukaDesktopAssistant.Services;

public sealed record ConversationMessage(string Role, string Text, DateTime Timestamp);

public sealed class ConversationStore
{
    private readonly string _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RukaDesktopAssistant");
    private readonly string _file;
    private readonly List<ConversationMessage> _messages = [];

    public IReadOnlyList<ConversationMessage> Messages => _messages;

    public ConversationStore()
    {
        _file = Path.Combine(_directory, "conversation.json");
        Load();
    }

    public void Add(string role, string text)
    {
        _messages.Add(new ConversationMessage(role, text, DateTime.Now));
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var data = JsonSerializer.Deserialize<List<ConversationMessage>>(File.ReadAllText(_file));
            if (data is not null) _messages.AddRange(data);
        }
        catch { /* Corrupt history must not prevent Ruka from starting. */ }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_file, JsonSerializer.Serialize(_messages, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
