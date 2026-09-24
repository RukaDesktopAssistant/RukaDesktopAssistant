using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RukaDesktopAssistant.Models;
using RukaDesktopAssistant.Services;
using RukaDesktopAssistant.Services.AI;
using Forms = System.Windows.Forms;

namespace RukaDesktopAssistant;

public partial class MainWindow : Window
{
    private readonly RukaState _state = new();
    private readonly ConversationStore _conversationStore = new();
    private readonly MemoryStore _memoryStore = new();
    private readonly PersonalityStore _personalityStore = new();
    private readonly PermissionManager _permissionManager = new();
    private readonly PcActionService _pcActions;
    private readonly VoiceService _voice = new();
    private readonly SettingsStore _settings = new();
    private readonly AppearanceSettings _appearance = new();
    private readonly AudioSettingsStore _audioStore = new();
    private readonly WakeWordService _wakeWord = new();
    private readonly VoiceInputService _voiceInput;
    private readonly ActivityScheduler _activityScheduler;
    private readonly CharacterController _character;
    private readonly MonitorProfileStore _monitorProfiles = new();
    private readonly GameProfileStore _gameProfiles = new();
    private readonly ShortcutService _shortcuts;
    private readonly CharacterAssetService _assets = new();
    private CharacterAnimationService? _animation;
    private System.Windows.Point _dragStart;
    private bool _dragging;
    private ChatWindow? _chatWindow;

    public MainWindow()
    {
        InitializeComponent();
        _settings.Load();
        _permissionManager.Load();
        _appearance.Load();
        _audioStore.Load();

        _wakeWord.WakePhrase = _audioStore.Current.WakePhrase;
        _wakeWord.Enabled = _audioStore.Current.WakeWordEnabled;
        _voice.Rate = _audioStore.Current.TtsRate;
        _voice.Volume = _audioStore.Current.TtsVolume;
        _voice.Enabled = _audioStore.Current.TtsEnabled;

        _pcActions = new PcActionService(_permissionManager);
        _character = new CharacterController(this);
        _activityScheduler = new ActivityScheduler(_state, Say, _character, _gameProfiles);
        _activityScheduler.ContextChanged += OnContextChanged;
        _activityScheduler.ActivityChanged += OnActivityChanged;
        _shortcuts = new ShortcutService(this);
        _voiceInput = new VoiceInputService(_wakeWord);

        _voiceInput.Recognized += OnVoiceRecognized;
        _voiceInput.Error += ex => Say($"音声入力を開始できなかったよ。{ex.Message}");

        Loaded += (_, _) =>
        {
            ApplyAppearance();
            RestorePosition();
            LoadCharacterAsset();
            ((Storyboard)FindResource("IdleFloat")).Begin(this, true);
            _shortcuts.Start();
            ApplyStartupSetting();

            if (_settings.AutonomousBehaviorEnabled)
                _activityScheduler.Start();

            if (_settings.VoiceInputEnabled)
                StartVoice();
        };

        Closed += (_, _) =>
        {
            SavePosition();
            _shortcuts.Dispose();
            _voiceInput.Dispose();
            _voice.Dispose();
        };

        _shortcuts.Pause += TogglePause;
        _shortcuts.OpenChat += () => OpenChat();
        _shortcuts.EmergencyStop += EmergencyStop;
    }

    private void ApplyAppearance()
    {
        Topmost = _appearance.AlwaysOnTop;
        Opacity = _appearance.Opacity;
        var scale = Math.Clamp(_appearance.Scale, 0.5, 2.0);
        CharacterVisual.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        if (CharacterVisual.RenderTransform is System.Windows.Media.ScaleTransform st)
        {
            st.ScaleX = scale;
            st.ScaleY = scale;
        }
        Bubble.Visibility = Visibility.Collapsed;
    }

    private void LoadCharacterAsset()
    {
        _animation = new CharacterAnimationService(_assets, CharacterImage);
        _animation.SetState("idle");
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

    private void OnActivityChanged(string activity)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => OnActivityChanged(activity));
            return;
        }

        _animation?.SetState(activity);
        if (activity == "walk" && !_state.IsPaused)
            _character.Wander(TimeSpan.FromSeconds(2.5));
        else if (activity == "sleep")
        {
            _voice.Stop();
            Bubble.Visibility = Visibility.Collapsed;
        }
    }

    private void OnContextChanged(AppContextInfo context, GameProfile profile)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => OnContextChanged(context, profile));
            return;
        }

        var inGame = context.IsKnownGame;
        var hasCustomGameProfile = context.ProcessName is not null && _gameProfiles.TryGet(context.ProcessName, out _);
        var show = !inGame || (hasCustomGameProfile ? profile.ShowCharacter : _settings.ShowDuringGames);
        Character.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        if (show && inGame && profile.MoveToSide)
            MoveToSide();

        var scale = inGame ? profile.Scale : _appearance.Scale;
        if (CharacterVisual.RenderTransform is System.Windows.Media.ScaleTransform st)
        {
            st.ScaleX = Math.Clamp(scale, 0.5, 2.0);
            st.ScaleY = Math.Clamp(scale, 0.5, 2.0);
        }

        if (inGame && !profile.VoiceEnabled)
            _voice.Stop();
    }

    private void MoveToSide()
    {
        var area = Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle).WorkingArea;
        _character.MoveTo(area.Right - Width - 24, area.Bottom - Height - 24, TimeSpan.FromMilliseconds(500));
    }

    private void OnVoiceRecognized(string text)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (text == "__RUKA_PAUSE__")
            {
                if (!_state.IsPaused) TogglePause();
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Speak("ん？どうした？");
                return;
            }

            OpenChat().SubmitVoiceText(text);
        });
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
        var screens = MonitorService.GetMonitors();
        if (screens.Count > 0)
        {
            var target = screens.FirstOrDefault(x => _monitorProfiles.Get(x.Id).Enabled) ?? screens[0];
            var profile = _monitorProfiles.Get(target.Id);
            var area = target.WorkingArea;
            if (profile.Position.Equals("bottom-right", StringComparison.OrdinalIgnoreCase))
            {
                Left = area.Right - Width - 40;
                Top = area.Bottom - Height - 40;
                return;
            }
        }
        var area = Forms.Screen.PrimaryScreen?.WorkingArea;
        if (area is null) return;
        var parts = _appearance.Position.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 &&
            double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var savedLeft) &&
            double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var savedTop))
        {
            Left = Math.Clamp(savedLeft, area.Value.Left, area.Value.Right - Width);
            Top = Math.Clamp(savedTop, area.Value.Top, area.Value.Bottom - Height);
            return;
        }
        Left = area.Value.Right - Width - 40;
        Top = area.Value.Bottom - Height - 40;
    }

    private void SavePosition()
    {
        _appearance.Position = $"{Left:0},{Top:0}";
        _appearance.Save();
    }

    private void Character_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            OpenChat();
            e.Handled = true;
            return;
        }
        _dragStart = e.GetPosition(this);
        _dragging = true;
        CaptureMouse();
        MouseMove += DragMove;
        MouseLeftButtonUp += EndDrag;
    }

    private void DragMove(object? sender, System.Windows.Input.MouseEventArgs e)
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
        SavePosition();
    }

    private void Character_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = new System.Windows.Controls.ContextMenu();
        menu.Items.Add(MenuItem("チャットを開く", (_, _) => OpenChat()));
        menu.Items.Add(MenuItem("設定", (_, _) => new SettingsWindow().Show()));
        menu.Items.Add(MenuItem("声で話す", (_, _) => Speak("ん？どうした？")));
        menu.Items.Add(MenuItem("音声入力を開始", (_, _) => StartVoice()));
        menu.Items.Add(MenuItem("音声入力を停止", (_, _) => _voiceInput.StopConversation()));
        menu.Items.Add(MenuItem("少し歩く", (_, _) => _character.Wander()));
        menu.Items.Add(new System.Windows.Controls.Separator());
        menu.Items.Add(MenuItem(_state.IsPaused ? "るかを再開" : "るかを待機", (_, _) => TogglePause()));
        menu.Items.Add(MenuItem("緊急停止", (_, _) => EmergencyStop()));
        menu.Items.Add(new System.Windows.Controls.Separator());
        menu.Items.Add(MenuItem("終了", (_, _) => System.Windows.Application.Current.Shutdown()));
        menu.IsOpen = true;
    }

    private static System.Windows.Controls.MenuItem MenuItem(string text, RoutedEventHandler action)
    {
        var item = new System.Windows.Controls.MenuItem { Header = text };
        item.Click += action;
        return item;
    }

    private void StartVoice()
    {
        if (_voiceInput.Start())
            Say($"聞いてるよ。『{_wakeWord.WakePhrase}』って呼んでね。");
    }

    private IAiProvider CreateAiProvider()
    {
        var providerSettings = new ProviderSettings();
        providerSettings.Load();
        var personality = _personalityStore.Profiles.FirstOrDefault(x => x.Name.Equals(_settings.ActivePersonalityName, StringComparison.OrdinalIgnoreCase))
            ?? _personalityStore.Profiles.FirstOrDefault()
            ?? new PersonalityProfile("るか", "あなたはデスクトップに住むAIアシスタント「るか」です。自然で親しみやすい日本語で答え、必要以上に長く話しません。PC操作はユーザーの明示的な依頼がある場合だけ提案してください。", "私", "自然でラフ");
        var memory = _memoryStore.Memories.Count == 0
            ? "長期記憶はありません。"
            : "ユーザーが明示的に保存した長期記憶:\n" + string.Join("\n", _memoryStore.Memories.Take(30).Select(x => "- " + x));
        var systemPrompt = $"""
        {personality.SystemPrompt}
        名前: {personality.Name}
        一人称: {personality.FirstPerson}
        話し方: {personality.SpeechStyle}
        {memory}
        ユーザーが「覚えて」と明示した内容だけを長期記憶として扱います。
        """;

        if ((providerSettings.Provider.Equals("openai", StringComparison.OrdinalIgnoreCase) ||
             providerSettings.Provider.Equals("http", StringComparison.OrdinalIgnoreCase)) &&
            Uri.TryCreate(providerSettings.Endpoint, UriKind.Absolute, out var endpoint) &&
            !string.IsNullOrWhiteSpace(providerSettings.EffectiveApiKey))
        {
            return new OpenAiCompatibleProvider(
                new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(60) },
                endpoint.ToString(),
                providerSettings.EffectiveApiKey,
                providerSettings.Model,
                systemPrompt,
                providerSettings.HistoryCount,
                providerSettings.SendRecentHistory);
        }
        return new LocalAiProvider();
    }

    private ChatWindow OpenChat()
    {
        if (_chatWindow is { IsVisible: true })
        {
            _chatWindow.Activate();
            return _chatWindow;
        }

        _chatWindow = new ChatWindow(_conversationStore, CreateAiProvider(), _memoryStore, _pcActions, Speak);
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
            _voice.Stop();
        }
        else
        {
            if (_settings.AutonomousBehaviorEnabled) _activityScheduler.Start();
            if (_settings.VoiceInputEnabled) StartVoice();
        }
        Speak(_state.IsPaused ? "ちょっと待機するね。" : "戻ったよ！");
    }

    private void EmergencyStop()
    {
        _state.IsPaused = true;
        _activityScheduler.Stop();
        _voiceInput.StopConversation();
        _voice.Stop();
        _animation?.SetState("idle");
        Bubble.Visibility = Visibility.Collapsed;
    }

    private void Speak(string text)
    {
        if (_appearance.ShowSpeechBubble)
        {
            BubbleText.Text = text;
            Bubble.Visibility = Visibility.Visible;
        }
        _animation?.SetState("talk");
        _voice.Speak(text);
        ((Storyboard)FindResource("TalkPulse")).Begin(this, true);
    }

    public void Say(string text) => Speak(text);
}