using System.Windows;
using System.Windows.Controls;
using RukaDesktopAssistant.Services;

namespace RukaDesktopAssistant;

public partial class ChatWindow : Window
{
    private readonly ConversationStore _store;

    public ChatWindow() : this(new ConversationStore()) { }

    public ChatWindow(ConversationStore store)
    {
        InitializeComponent();
        _store = store;
        Loaded += (_, _) => LoadHistory();
        Input.Focus();
    }

    private void LoadHistory()
    {
        foreach (var message in _store.Messages.TakeLast(100))
            AddMessage(message.Role == "user" ? "あなた" : "るか", message.Text);
    }

    private void Send_Click(object sender, RoutedEventArgs e)
    {
        var text = Input.Text.Trim();
        if (text.Length == 0) return;
        _store.Add("user", text);
        AddMessage("あなた", text);
        Input.Clear();

        var reply = CreateLocalReply(text);
        _store.Add("assistant", reply);
        AddMessage("るか", reply);
    }

    private static string CreateLocalReply(string text)
    {
        if (text.Contains("ねぇ、るか") || text.Contains("るか")) return "ん？どうした？";
        if (text.Contains("こんにちは")) return "こんにちは！今日もよろしくね。";
        if (text.Contains("ありがとう")) return "どういたしまして！";
        return "受け取ったよ。AI接続を追加すると、ここで本格的に会話できるようになるよ。";
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
