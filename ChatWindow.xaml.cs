using System.Windows;
using System.Windows.Controls;
using RukaDesktopAssistant.Services;
using RukaDesktopAssistant.Services.AI;

namespace RukaDesktopAssistant;

public partial class ChatWindow : Window
{
    private readonly ConversationStore _store;
    private readonly AiConversationService _ai;
    private readonly SemaphoreSlim _replyLock = new(1, 1);
    private readonly MemoryStore _memory;
    private readonly PcActionService? _pcActions;

    public ChatWindow()
        : this(new ConversationStore(), new LocalAiProvider(), new MemoryStore(), null) { }

    public ChatWindow(ConversationStore store)
        : this(store, new LocalAiProvider(), new MemoryStore(), null) { }

    public ChatWindow(
        ConversationStore store,
        IAiProvider provider,
        MemoryStore? memory = null,
        PcActionService? pcActions = null)
    {
        InitializeComponent();
        _store = store;
        _ai = new AiConversationService(provider);
        _memory = memory ?? new MemoryStore();
        _pcActions = pcActions;
        Loaded += (_, _) => LoadHistory();
        Input.Focus();
    }

    public void SubmitVoiceText(string text)
    {
        Input.Text = text;
        Send_Click(this, new RoutedEventArgs());
    }

    private void LoadHistory()
    {
        foreach (var message in _store.Messages.TakeLast(100))
            AddMessage(message.Role == "user" ? "あなた" : "るか", message.Text);
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        if (!await _replyLock.WaitAsync(0)) return;

        try
        {
            var text = Input.Text.Trim();
            if (text.Length == 0) return;

            _store.Add("user", text);
            AddMessage("あなた", text);
            Input.Clear();

            var memoryCommand = TryHandleMemoryCommand(text);
            if (memoryCommand is not null)
            {
                _store.Add("assistant", memoryCommand);
                AddMessage("るか", memoryCommand);
                return;
            }

            if (_pcActions is not null)
            {
                var actionReply = await _pcActions.TryHandleAsync(text, ConfirmDangerousActionAsync);
                if (!string.IsNullOrWhiteSpace(actionReply))
                {
                    _store.Add("assistant", actionReply);
                    AddMessage("るか", actionReply);
                    return;
                }
            }

            var history = _store.Messages
                .TakeLast(20)
                .Select(m => $"{m.Role}: {m.Text}")
                .ToArray();

            var reply = await _ai.ReplyAsync(text, history);
            _store.Add("assistant", reply);
            AddMessage("るか", reply);
        }
        catch (Exception ex)
        {
            var reply = $"ごめん、返答中にエラーが起きたよ。{ex.Message}";
            _store.Add("assistant", reply);
            AddMessage("るか", reply);
        }
        finally
        {
            _replyLock.Release();
        }
    }

    private string? TryHandleMemoryCommand(string text)
    {
        var normalized = text.Trim();

        if (normalized.StartsWith("覚えて", StringComparison.OrdinalIgnoreCase))
        {
            var memoryText = normalized["覚えて".Length..]
                .Trim(' ', '　', '、', '。', '！', '!');
            if (string.IsNullOrWhiteSpace(memoryText))
                return "もちろん。覚えてほしい内容を続けて教えてね。";

            _memory.Remember(memoryText);
            return $"覚えておくね。「{memoryText}」";
        }

        if (normalized.StartsWith("忘れて", StringComparison.OrdinalIgnoreCase))
        {
            var memoryText = normalized["忘れて".Length..]
                .Trim(' ', '　', '、', '。', '！', '!');
            if (string.IsNullOrWhiteSpace(memoryText))
                return "どの記憶を忘れればいい？";

            _memory.Forget(memoryText);
            return $"その内容は長期記憶から外したよ。「{memoryText}」";
        }

        if (normalized is "覚えていること" or "何を覚えてる？" or "何を覚えている？")
        {
            if (_memory.Memories.Count == 0)
                return "今は長期記憶に保存していることはないよ。";

            return "今保存している長期記憶はこれだよ：\n" +
                   string.Join("\n", _memory.Memories.Select(x => "・" + x));
        }

        return null;
    }

    private Task<bool> ConfirmDangerousActionAsync(string description)
    {
        var result = System.Windows.MessageBox.Show(
            description + "\n\n実行していい？",
            "るか — 操作の確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    private void AddMessage(string speaker, string text)
    {
        Messages.Children.Add(new TextBlock
        {
            Text = $"{speaker}: {text}",
            Margin = new Thickness(0, 6, 0, 6),
            TextWrapping = TextWrapping.Wrap
        });
        History.ScrollToEnd();
    }
}
