using System.Threading;
using System.Windows;

namespace RukaDesktopAssistant;

public partial class App : System.Windows.Application
{
    private static Mutex? _singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        _singleInstance = new Mutex(true, "RukaDesktopAssistant.SingleInstance", out var created);
        if (!created)
        {
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
