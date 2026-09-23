using System.Windows;

namespace RukaDesktopAssistant.Services;

public sealed class NotificationService
{
    public bool Enabled { get; set; } = true;
    public void Notify(string title, string message)
    {
        if (!Enabled) return;
        // Uses the WPF application shell for now; native Windows toast integration is isolated for the next layer.
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
