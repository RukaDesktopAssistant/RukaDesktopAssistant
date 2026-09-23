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

    public ChatWindow() : this(new ConversationStore(), new LocalAiProvider()) { }

    public ChatWindow(ConversationStore store)
        : this(store, new LocalAiProvider()) { }

    public ChatWindow(ConversationStore store, IAiProvider provider)
    {
        InitializeComponent();
        _store = store;
        _ai = new AiConversationService(provider);
        Loaded += (_, _) => LoadHistory();
        Input.Focus();
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
