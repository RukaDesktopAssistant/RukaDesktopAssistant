using System.Windows;
using RukaDesktopAssistant.Services;

namespace RukaDesktopAssistant;

public partial class SettingsWindow : Window
{
    private readonly PermissionManager _permissions = new();
    private readonly SettingsStore _settings = new();
    private readonly AppearanceSettings _appearance = new();
    private readonly AudioSettingsStore _audioStore = new();
    private readonly ProviderSettings _providerSettings = new();
    private readonly WakeWordService _wake = new();

    public SettingsWindow()
    {
        InitializeComponent();

        _settings.Load();
        _permissions.Load();
        _appearance.Load();
        _audioStore.Load();
        _providerSettings.Load();

        Navigation.SelectedIndex = 0;
        LoadControls();
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

        VoiceInputEnabled.IsChecked = _settings.VoiceInputEnabled;
        WakeEnabled.IsChecked = _audioStore.Current.WakeWordEnabled;
        WakePhrase.Text = _audioStore.Current.WakePhrase;
        TtsEnabled.IsChecked = _audioStore.Current.TtsEnabled;
        VoiceRate.Value = _audioStore.Current.TtsRate;
        VoiceVolume.Value = _audioStore.Current.TtsVolume;

        AiProvider.SelectedItem = AiProvider.Items
            .OfType<System.Windows.Controls.ComboBoxItem>()
            .FirstOrDefault(x => string.Equals(x.Content?.ToString(), _providerSettings.Provider, StringComparison.OrdinalIgnoreCase))
            ?? AiProvider.Items[0];
        AiEndpoint.Text = _providerSettings.Endpoint;
        SendHistory.IsChecked = _providerSettings.SendRecentHistory;
        HistoryCount.Value = _providerSettings.HistoryCount;

        AllowScreenRead.IsChecked = _permissions.AllowScreenRead;
        AllowLaunchApps.IsChecked = _permissions.AllowLaunchApps;
        AllowCloseApps.IsChecked = _permissions.AllowCloseApps;
        AllowFileRead.IsChecked = _permissions.AllowFileRead;
        AllowFileWrite.IsChecked = _permissions.AllowFileWrite;
        AllowBrowserControl.IsChecked = _permissions.AllowBrowserControl;
        AllowSystemSettings.IsChecked = _permissions.AllowSystemSettings;

        ScaleSlider.ValueChanged += (_, _) => UpdateAppearanceLabels();
        OpacitySlider.ValueChanged += (_, _) => UpdateAppearanceLabels();
    }

    private void UpdateAppearanceLabels()
    {
        if (ScaleValue is null) return;
        ScaleValue.Text = $"サイズ: {ScaleSlider.Value:0.0}x";
        OpacityValue.Text = $"透明度: {OpacitySlider.Value:P0}";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.StartWithWindows = StartWithWindows.IsChecked == true;
        _settings.ShowDuringGames = ShowDuringGames.IsChecked == true;
        _settings.DiscordWakeWordOnly = DiscordWakeWordOnly.IsChecked == true;
        _settings.AutonomousBehaviorEnabled = AutonomousBehavior.IsChecked == true;
        _settings.VoiceInputEnabled = VoiceInputEnabled.IsChecked == true;
        _settings.Save();

        _audioStore.Current.WakeWordEnabled = WakeEnabled.IsChecked == true;
        _audioStore.Current.WakePhrase = string.IsNullOrWhiteSpace(WakePhrase.Text) ? "ねぇ、るか" : WakePhrase.Text.Trim();
        _audioStore.Current.TtsEnabled = TtsEnabled.IsChecked == true;
        _audioStore.Current.TtsRate = (int)Math.Round(VoiceRate.Value);
        _audioStore.Current.TtsVolume = (int)Math.Round(VoiceVolume.Value);
        _audioStore.Save();

        _providerSettings.Provider = (AiProvider.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "local";
        _providerSettings.Endpoint = AiEndpoint.Text.Trim();
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

        MessageBox.Show("設定を保存したよ。再起動すると、るか本体にもすべて反映されるよ。", "るか",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Navigation_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (PageTitle is null || Navigation.SelectedItem is not System.Windows.Controls.ListBoxItem item) return;

        var title = item.Content?.ToString() ?? "設定";
        PageTitle.Text = title;
        PageDescription.Text = title switch
        {
            "PC操作" => "るかがPCを操作できる範囲を管理します。危険な操作は別途確認します。",
            "権限" => "各PC操作権限を個別にオン・オフできます。",
            "記憶" => "会話履歴と長期記憶を分離して管理します。",
            "ゲーム" => "ゲームごとの表示・サイズ・位置・音声反応を管理します。",
            "音声" => "マイク入力、ウェイクワード、音声合成を管理します。",
            "外観" => "るかのサイズと透明度を管理します。",
            "AI" => "接続するAIプロバイダーを管理します。",
            "通知" => "通知の表示方法を管理します。",
            _ => "るかの動作をWindows設定のように管理します。"
        };
    }
}
