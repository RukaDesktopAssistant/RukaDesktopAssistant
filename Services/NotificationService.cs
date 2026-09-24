using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

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
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            try
            {
                using var icon = new NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Application,
                    Visible = true,
                    BalloonTipTitle = title,
                    BalloonTipText = message
                };
                icon.ShowBalloonTip(5000);
                _ = Task.Delay(6000).ContinueWith(_ => icon.Dispose());
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });
    }
}