using System.Windows;
using System.Windows.Controls;
using RukaDesktopAssistant.Services;
using RukaDesktopAssistant.Services.AI;

namespace RukaDesktopAssistant;

public partial class SettingsWindow : Window
{
    private readonly PermissionManager _permissions = new();
    private readonly SettingsStore _settings = new();
    private readonly AppearanceSettings _appearance = new();
    private readonly AudioSettingsStore _audioStore = new();
    private readonly ProviderSettings _providerSettings = new();
    private readonly ConversationStore _conversation = new();
    private readonly MemoryStore _memory = new();
    private readonly GameProfileStore _games = new();

    private readonly Dictionary<string, FrameworkElement> _pages;

    public SettingsWindow()
    {
        InitializeComponent();

        _settings.Load();
        _permissions.Load();
        _appearance.Load();
        _audioStore.Load();
        _providerSettings.Load();

        _pages = new()
        {
            ["システム"] = SystemPage, ["外観"] = AppearancePage, ["会話"] = ConversationPage,
            ["音声"] = VoicePage, ["AI"] = AiPage, ["記憶"] = MemoryPage, ["PC操作"] = PcPage,
            ["ゲーム"] = GamePage, ["通知"] = NotificationPage, ["キャラクター"] = CharacterPage,
            ["権限"] = PermissionPage, ["ショートカット"] = ShortcutPage
        };

        LoadControls();
        Navigation.SelectedIndex = 0;
    }

    private void LoadControls()
    {
        StartWithWindows.IsChecked = _settings.StartWithWindows;
        ShowDuringGames.IsChecked = _settings.ShowDuringGames;
        DiscordWakeWordOnly.IsChecked = _settings.DiscordWakeWordOnly;
        AutonomousBehavior.IsChecked = _settings.AutonomousBehaviorEnabled;
        DangerousConfirmation.IsChecked = _permissions.RequireConfirmationForDangerousActions;

        ScaleSlider.Value = _appearance.Scale;
        OpacitySlider.Value = _appearance.Opacity;
        UpdateAppearanceLabels();
        ScaleSlider.ValueChanged += (_, _) => UpdateAppearanceLabels();
        OpacitySlider.ValueChanged += (_, _) => UpdateAppearanceLabels();

        VoiceInputEnabled.IsChecked = _settings.VoiceInputEnabled;
        WakeEnabled.IsChecked = _audioStore.Current.WakeWordEnabled;
        WakePhrase.Text = _audioStore.Current.WakePhrase;
        TtsEnabled.IsChecked = _audioStore.Current.TtsEnabled;
        VoiceRate.Value = _audioStore.Current.TtsRate;
        VoiceVolume.Value = _audioStore.Current.TtsVolume;

        AiProvider.SelectedItem = AiProvider.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(x => string.Equals(x.Content?.ToString(), _providerSettings.Provider, StringComparison.OrdinalIgnoreCase))
            ?? AiProvider.Items[0];
        AiEndpoint.Text = _providerSettings.Endpoint;
        AiApiKey.Text = _providerSettings.ApiKey;
        AiModel.Text = _providerSettings.Model;
        SendHistory.IsChecked = _providerSettings.SendRecentHistory;
        HistoryCount.Value = _providerSettings.HistoryCount;

        AllowScreenRead.IsChecked = _permissions.AllowScreenRead;
        AllowLaunchApps.IsChecked = _permissions.AllowLaunchApps;
        AllowCloseApps.IsChecked = _permissions.AllowCloseApps;
        AllowFileRead.IsChecked = _permissions.AllowFileRead;
        AllowFileWrite.IsChecked = _permissions.AllowFileWrite;
        AllowBrowserControl.IsChecked = _permissions.AllowBrowserControl;
        AllowSystemSettings.IsChecked = _permissions.AllowSystemSettings;

        NotificationsEnabled.IsChecked = _settings.NotificationsEnabled;
        RefreshMemoryList();
        LoadGameProfile();
    }

    private void UpdateAppearanceLabels()
    {
        ScaleValue.Text = $"サイズ: {ScaleSlider.Value:0.0}x";
        OpacityValue.Text = $"透明度: {OpacitySlider.Value:P0}";
    }

    private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Navigation.SelectedItem is not ListBoxItem item) return;
        var title = item.Content?.ToString() ?? "設定";
        foreach (var page in _pages.Values) page.Visibility = Visibility.Collapsed;
        if (_pages.TryGetValue(title, out var selected)) selected.Visibility = Visibility.Visible;

        PageTitle.Text = title;
        PageDescription.Text = title switch
        {
            "システム" => "起動・自律動作・ゲーム中の基本動作。",
            "外観" => "サイズと透明度など、デスクトップ上の見た目。",
            "会話" => "会話履歴を長期記憶とは別に管理。",
            "音声" => "マイク入力、ウェイクワード、音声合成。",
            "AI" => "AI接続と実通信テスト。",
            "記憶" => "明示的に保存した長期記憶だけを管理。",
            "PC操作" => "るかが実行できるPC操作の概要。",
            "ゲーム" => "ゲームごとに表示・位置・サイズ・音声を設定。",
            "通知" => "通知のオン・オフ。",
            "キャラクター" => "画像アセットと将来のモデル拡張。",
            "権限" => "PC操作権限を個別に管理。",
            "ショートカット" => "グローバルショートカット一覧。",
            _ => "るかの動作を管理します。"
        };
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.StartWithWindows = StartWithWindows.IsChecked == true;
        _settings.ShowDuringGames = ShowDuringGames.IsChecked == true;
        _settings.DiscordWakeWordOnly = DiscordWakeWordOnly.IsChecked == true;
        _settings.AutonomousBehaviorEnabled = AutonomousBehavior.IsChecked == true;
        _settings.VoiceInputEnabled = VoiceInputEnabled.IsChecked == true;
        _settings.NotificationsEnabled = NotificationsEnabled.IsChecked == true;
        _settings.Save();

        _audioStore.Current.WakeWordEnabled = WakeEnabled.IsChecked == true;
        _audioStore.Current.WakePhrase = string.IsNullOrWhiteSpace(WakePhrase.Text) ? "ねぇ、るか" : WakePhrase.Text.Trim();
        _audioStore.Current.TtsEnabled = TtsEnabled.IsChecked == true;
        _audioStore.Current.TtsRate = (int)Math.Round(VoiceRate.Value);
        _audioStore.Current.TtsVolume = (int)Math.Round(VoiceVolume.Value);
        _audioStore.Save();

        _providerSettings.Provider = (AiProvider.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "local";
        _providerSettings.Endpoint = AiEndpoint.Text.Trim();
        _providerSettings.ApiKey = AiApiKey.Text.Trim();
        _providerSettings.Model = string.IsNullOrWhiteSpace(AiModel.Text) ? "gpt-4o-mini" : AiModel.Text.Trim();
        _providerSettings.SendRecentHistory = SendHistory.IsChecked == true;
        _providerSettings.HistoryCount = (int)Math.Round(HistoryCount.Value);
        _providerSettings.Save();

        _permissions.RequireConfirmationForDangerousActions = DangerousConfirmation.IsChecked == true;
        _permissions.AllowScreenRead = AllowScreenRead.IsChecked == true;
        _permissions.AllowLaunchApps = AllowLaunchApps.IsChecked == true;
        _permissions.AllowCloseApps = AllowCloseApps.IsChecked == true;
        _permissions.AllowFileRead = AllowFileRead.IsChecked == true;
        _permissions.AllowFileWrite = AllowFileWrite.IsChecked == true;
        _permissions.AllowBrowserControl = AllowBrowserControl.IsChecked == true;
        _permissions.AllowSystemSettings = AllowSystemSettings.IsChecked == true;
        _permissions.Save();

        _appearance.Scale = ScaleSlider.Value;
        _appearance.Opacity = OpacitySlider.Value;
        _appearance.Save();

        MessageBox.Show("設定を保存したよ。", "るか", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void TestAi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var provider = (AiProvider.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "local";
            if (!provider.Equals("openai", StringComparison.OrdinalIgnoreCase) &&
                !provider.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("実通信テストには openai / http を選んでね。", "るか");
                return;
            }

            var key = string.IsNullOrWhiteSpace(AiApiKey.Text)
                ? Environment.GetEnvironmentVariable("RUKA_AI_API_KEY") ?? ""
                : AiApiKey.Text.Trim();

            if (!Uri.TryCreate(AiEndpoint.Text.Trim(), UriKind.Absolute, out var endpoint))
                throw new InvalidOperationException("APIエンドポイントが正しくありません。");

            var test = new OpenAiCompatibleProvider(
                new HttpClient { Timeout = TimeSpan.FromSeconds(20) },
                endpoint.ToString(), key,
                string.IsNullOrWhiteSpace(AiModel.Text) ? "gpt-4o-mini" : AiModel.Text.Trim(),
                "接続テスト用アシスタント。短く日本語で返答してください。");

            var reply = await test.GenerateAsync("接続テスト。成功したら「接続OK」とだけ返してください。", []);
            MessageBox.Show($"AI接続成功。実際の返答：\n{reply}", "るか");
        }
        catch (Exception ex)
        {
            MessageBox.Show("AI接続テスト失敗。\n" + ex.Message, "るか", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RefreshMemoryList()
    {
        MemoryList.Items.Clear();
        foreach (var item in _memory.Memories) MemoryList.Items.Add(item);
    }

    private void AddMemory_Click(object sender, RoutedEventArgs e)
    {
        var text = MemoryInput.Text.Trim();
        if (text.Length == 0) return;
        _memory.Remember(text);
        MemoryInput.Clear();
        RefreshMemoryList();
    }

    private void DeleteMemory_Click(object sender, RoutedEventArgs e)
    {
        if (MemoryList.SelectedItem is string text)
        {
            _memory.Forget(text);
            RefreshMemoryList();
        }
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("会話履歴をすべて削除する？", "るか", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _conversation.Clear();
        MessageBox.Show("会話履歴を削除したよ。長期記憶は残してあるよ。", "るか");
    }

    private void SaveGame_Click(object sender, RoutedEventArgs e)
    {
        var name = GameName.Text.Trim();
        if (name.Length == 0) return;
        _games.Set(name, new GameProfile(
            GameShow.IsChecked == true,
            GameScale.Value,
            "side",
            GameVoice.IsChecked == true,
            GameMoveSide.IsChecked == true));
        MessageBox.Show($"{name} の設定を保存したよ。", "るか");
    }

    private void LoadGameProfile()
    {
        if (GameName is null) return;
        GameName.SelectionChanged += (_, _) => LoadSelectedGame();
        GameName.LostFocus += (_, _) => LoadSelectedGame();
        LoadSelectedGame();
    }

    private void LoadSelectedGame()
    {
        var name = GameName.Text.Trim();
        if (name.Length == 0) return;
        var p = _games.Get(name);
        GameShow.IsChecked = p.ShowCharacter;
        GameMoveSide.IsChecked = p.MoveToSide;
        GameVoice.IsChecked = p.VoiceEnabled;
        GameScale.Value = p.Scale;
    }
}
