using System.Windows;

namespace RukaDesktopAssistant.Services;

public sealed class NotificationService
{
    public bool Enabled { get; set; } = true;
    public event Action<string, string>? Requested;

    public void Notify(string title, string message)
    {
        if (!Enabled) return;

        // Keep the notification transport replaceable. The desktop shell can subscribe
        // and route this to native Windows notifications without coupling core logic to UI.
        Requested?.Invoke(title, message);

        if (Application.Current?.Dispatcher is null) return;
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Application.Current.Windows.OfType<Window>().Any(w => w.IsActive))
                return;
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }
}
