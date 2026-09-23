using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace RukaDesktopAssistant;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _idleTimer;
    private bool _paused;
    private Point _dragStart;
    private double _windowLeft;
    private double _windowTop;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => RestorePosition();
        _idleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _idleTimer.Tick += (_, _) => IdleTick();
        _idleTimer.Start();
    }

    private void RestorePosition()
    {
        var area = Forms.Screen.PrimaryScreen?.WorkingArea;
        if (area is null) return;
        Left = area.Value.Right - Width - 40;
        Top = area.Value.Bottom - Height - 40;
    }

    private void Character_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(this);
        CaptureMouse();
        MouseMove += DragMove;
        MouseLeftButtonUp += EndDrag;
    }

    private void DragMove(object? sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        var p = e.GetPosition(null);
        Left = p.X - _dragStart.X;
        Top = p.Y - _dragStart.Y;
    }

    private void EndDrag(object? sender, MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
        MouseMove -= DragMove;
        MouseLeftButtonUp -= EndDrag;
        _windowLeft = Left;
        _windowTop = Top;
    }

    private void Character_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = new ContextMenu();
        menu.Items.Add(MenuItem("チャットを開く", (_, _) => new ChatWindow().Show()));
        menu.Items.Add(MenuItem(_paused ? "るかを再開" : "るかを待機", (_, _) => TogglePause()));
        menu.Items.Add(MenuItem("終了", (_, _) => Application.Current.Shutdown()));
        menu.IsOpen = true;
    }

    private static MenuItem MenuItem(string text, RoutedEventHandler action)
    {
        var item = new MenuItem { Header = text };
        item.Click += action;
        return item;
    }

    private void TogglePause()
    {
        _paused = !_paused;
        if (_paused) _idleTimer.Stop(); else _idleTimer.Start();
        Say(_paused ? "ちょっと待機するね。" : "戻ったよ！");
    }

    private void IdleTick()
    {
        if (_paused) return;
        // Placeholder for autonomous behavior scheduler.
    }

    public void Say(string text)
    {
        BubbleText.Text = text;
        Bubble.Visibility = Visibility.Visible;
    }
}
