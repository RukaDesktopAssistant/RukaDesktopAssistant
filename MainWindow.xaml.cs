using System.Windows;
using System.Windows.Input;
using RukaDesktopAssistant.Models;
using RukaDesktopAssistant.Services;
using Forms = System.Windows.Forms;

namespace RukaDesktopAssistant;

public partial class MainWindow : Window
{
    private readonly RukaState _state = new();
    private readonly ConversationStore _conversationStore = new();
    private readonly VoiceService _voice = new();
    private readonly ActivityScheduler _activityScheduler;
    private readonly CharacterController _character;
    private Point _dragStart;
    private bool _dragging;

    public MainWindow()
    {
        InitializeComponent();
        _character = new CharacterController(this);
        _activityScheduler = new ActivityScheduler(_state, Say);
        Loaded += (_, _) => RestorePosition();
        Closed += (_, _) => _voice.Dispose();
        _activityScheduler.Start();
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
        _dragging = true;
        CaptureMouse();
        MouseMove += DragMove;
        MouseLeftButtonUp += EndDrag;
    }

    private void DragMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging || e.LeftButton != MouseButtonState.Pressed) return;
        var p = e.GetPosition(null);
        Left = p.X - _dragStart.X;
        Top = p.Y - _dragStart.Y;
    }

    private void EndDrag(object? sender, MouseButtonEventArgs e)
    {
        _dragging = false;
        ReleaseMouseCapture();
        MouseMove -= DragMove;
        MouseLeftButtonUp -= EndDrag;
    }

    private void Character_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = new ContextMenu();
        menu.Items.Add(MenuItem("チャットを開く", (_, _) => OpenChat()));
        menu.Items.Add(MenuItem("設定", (_, _) => new SettingsWindow().Show()));
        menu.Items.Add(MenuItem("声で話す", (_, _) => Speak("ん？どうした？")));
        menu.Items.Add(MenuItem("少し歩く", (_, _) => _character.Wander()));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem(_state.IsPaused ? "るかを再開" : "るかを待機", (_, _) => TogglePause()));
        menu.Items.Add(MenuItem("緊急停止", (_, _) => EmergencyStop()));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem("終了", (_, _) => Application.Current.Shutdown()));
        menu.IsOpen = true;
    }

    private static MenuItem MenuItem(string text, RoutedEventHandler action)
    {
        var item = new MenuItem { Header = text };
        item.Click += action;
        return item;
    }

    private void OpenChat() => new ChatWindow(_conversationStore).Show();

    private void TogglePause()
    {
        _state.IsPaused = !_state.IsPaused;
        if (_state.IsPaused) _activityScheduler.Stop(); else _activityScheduler.Start();
        Say(_state.IsPaused ? "ちょっと待機するね。" : "戻ったよ！");
    }

    private void EmergencyStop()
    {
        _state.IsPaused = true;
        _activityScheduler.Stop();
        Say("緊急停止したよ。");
    }

    public void Say(string text)
    {
        BubbleText.Text = text;
        Bubble.Visibility = Visibility.Visible;
        _voice.Speak(text);
    }
}
