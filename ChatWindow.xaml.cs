using System.Windows;
using System.Windows.Controls;

namespace RukaDesktopAssistant;

public partial class ChatWindow : Window
{
    public ChatWindow() { InitializeComponent(); Input.Focus(); }

    private void Send_Click(object sender, RoutedEventArgs e)
    {
        var text = Input.Text.Trim();
        if (text.Length == 0) return;
        AddMessage("あなた", text);
        Input.Clear();
        AddMessage("るか", "受け取ったよ。AI接続部分はこれから実装していくね！");
    }

    private void AddMessage(string speaker, string text)
    {
        Messages.Children.Add(new TextBlock { Text = $"{speaker}: {text}", Margin = new Thickness(0, 6, 0, 6), TextWrapping = TextWrapping.Wrap });
        History.ScrollToEnd();
    }
}
