using System.Windows;

namespace RukaDesktopAssistant.Services;

public sealed class NotificationService
{
    public bool Enabled { get; set; } = true;
    public event Action<string, string>? Requested;

    public void Notify(string title, string message)
    {
        if (!Enabled) return;

        Requested?.Invoke(title, message);

        if (System.Windows.Application.Current?.Dispatcher is null) return;
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (System.Windows.Application.Current.Windows.OfType<Window>().Any(w => w.IsActive))
                return;
            System.Windows.MessageBox.Show(
                message,
                title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        });
    }
}
