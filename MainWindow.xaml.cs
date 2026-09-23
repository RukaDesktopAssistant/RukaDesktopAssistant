using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RukaDesktopAssistant.Models;
using RukaDesktopAssistant.Services;
using Forms = System.Windows.Forms;

namespace RukaDesktopAssistant;

public partial class MainWindow : Window
{
    private readonly RukaState _state = new();
    private readonly ConversationStore _conversationStore = new();
    private readonly VoiceService _voice = new();
    private readonly SettingsStore _settings = new();
    private readonly WakeWordService _wakeWord = new();
    private readonly VoiceInputService _voiceInput;
    private readonly ActivityScheduler _activityScheduler;
    private readonly CharacterController _character;
    private readonly ShortcutService _shortcuts;
    private readonly CharacterAssetService _assets = new();
    private Point _dragStart;
    private bool _dragging;
    private ChatWindow? _chatWindow;

    public MainWindow()
    {
        InitializeComponent();

        _settings.Load();
        _wakeWord.WakePhrase = _settings.WakePhrase;
        _voice.Rate = (int)_settings.VoiceRate;
        _voice.Volume = (int)_settings.VoiceVolume;
        _voice.Enabled = true;

        _character = new CharacterController(this);
        _activityScheduler = new ActivityScheduler(_state, Say);
        _shortcuts = new ShortcutService(this);
        _voiceInput = new VoiceInputService(_wakeWord);

        _voiceInput.Recognized += OnVoiceRecognized;
        _voiceInput.Error += ex => Say($"音声入力を開始できなかったよ。{ex.Message}");

        Loaded += (_, _) =>
        {
            RestorePosition();
            LoadCharacterAsset();
            ((Storyboard)FindResource("IdleFloat")).Begin(this, true);
            _shortcuts.Start();
            ApplyStartupSetting();
            if (_settings.VoiceInputEnabled) StartVoice();
        };

        Closed += (_, _) =>
        {
            _shortcuts.Dispose();
            _voiceInput.Dispose();
            _voice.Dispose();
        };

        _shortcuts.Pause += TogglePause;
        _shortcuts.OpenChat += OpenChat;
        _shortcuts.EmergencyStop += EmergencyStop;
        _activityScheduler.Start();
    }

    private void LoadCharacterAsset()
    {
        var image = _assets.TryLoad("ruka-idle.png");
        if (image is null)
        {
            CharacterImage.Visibility = Visibility.Collapsed;
            CharacterFallback.Visibility = Visibility.Visible;
            return;
        }

        CharacterImage.Source = image;
        CharacterImage.Visibility = Visibility.Visible;
        CharacterFallback.Visibility = Visibility.Collapsed;
    }

    private void OnVoiceRecognized(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Say("ん？どうした？");
            return;
        }

        var chat = OpenChat();
        chat.SubmitVoiceText(text);
    }

    private void ApplyStartupSetting()
    {
        try
        {
            var startup = new StartupService();
            startup.SetEnabled(_settings.StartWithWindows,
                Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        catch { }
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
        menu.Items.Add(MenuItem("音声入力を開始", (_, _) => StartVoice()));
        menu.Items.Add(MenuItem("音声入力を停止", (_, _) => _voiceInput.StopConversation()));
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

    private void StartVoice()
    {
        if (_voiceInput.Start())
            Say("聞いてるよ。『ねぇ、るか』って呼んでね。");
    }

    private ChatWindow OpenChat()
    {
        if (_chatWindow is { IsVisible: true })
        {
            _chatWindow.Activate();
            return _chatWindow;
        }

        _chatWindow = new ChatWindow(_conversationStore);
        _chatWindow.Closed += (_, _) => _chatWindow = null;
        _chatWindow.Show();
        return _chatWindow;
    }

    private void TogglePause()
    {
        _state.IsPaused = !_state.IsPaused;
        if (_state.IsPaused)
        {
            _activityScheduler.Stop();
            _voiceInput.StopConversation();
        }
        else
        {
            _activityScheduler.Start();
        }

        Say(_state.IsPaused ? "ちょっと待機するね。" : "戻ったよ！");
    }

    private void EmergencyStop()
    {
        _state.IsPaused = true;
        _activityScheduler.Stop();
        _voiceInput.StopConversation();
        _voice.Stop();
        Say("緊急停止したよ。");
    }

    private void Speak(string text)
    {
        BubbleText.Text = text;
        Bubble.Visibility = Visibility.Visible;
        _voice.Speak(text);
        ((Storyboard)FindResource("TalkPulse")).Begin(this, true);
    }

    public void Say(string text) => Speak(text);
}
