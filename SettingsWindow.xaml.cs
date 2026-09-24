using System.Windows;
using System.Windows.Controls;
using RukaDesktopAssistant.Models;
using RukaDesktopAssistant.Services;
using RukaDesktopAssistant.Services.AI;

namespace RukaDesktopAssistant;

public partial class SettingsWindow : Window
{
    public event Action? SettingsSaved;
    private readonly PermissionManager _permissions = new();
    private readonly SettingsStore _settings = new();
    private readonly AppearanceSettings _appearance = new();
    private readonly AudioSettingsStore _audioStore = new();
    private readonly ProviderSettings _providerSettings = new();
    private readonly MemoryStore _memory = new();
    private readonly ConversationStore _conversation = new();
    private readonly PersonalityStore _personalities = new();
    private readonly GameProfileStore _games = new();
    private readonly MonitorProfileStore _monitors = new();
    private readonly string[] _gameNames = ["RobloxPlayerBeta","FortniteClient-Win64-Shipping","VALORANT-Win64-Shipping","r5apex","ApexLegends","Overwatch","Minecraft"];

    public SettingsWindow()
    {
        InitializeComponent();
        _settings.Load(); _permissions.Load(); _appearance.Load(); _audioStore.Load(); _providerSettings.Load();
        LoadControls(); Navigation.SelectedIndex = 0; LoadGameNames(); LoadPersonalities(); LoadMonitors(); RefreshMemory();
    }

    private void LoadControls()
    {
        StartWithWindows.IsChecked=_settings.StartWithWindows; AutonomousBehavior.IsChecked=_settings.AutonomousBehaviorEnabled; ShowDuringGames.IsChecked=_settings.ShowDuringGames; DiscordWakeWordOnly.IsChecked=_settings.DiscordWakeWordOnly;
        ScaleSlider.Value=_appearance.Scale; OpacitySlider.Value=_appearance.Opacity; AlwaysOnTop.IsChecked=_appearance.AlwaysOnTop; ShowBubble.IsChecked=_appearance.ShowSpeechBubble; CharacterAlwaysTop.IsChecked=_appearance.AlwaysOnTop; UpdateAppearanceLabels();
        ScaleSlider.ValueChanged+=(_,_)=>UpdateAppearanceLabels(); OpacitySlider.ValueChanged+=(_,_)=>UpdateAppearanceLabels();
        VoiceInputEnabled.IsChecked=_settings.VoiceInputEnabled; WakeEnabled.IsChecked=_audioStore.Current.WakeWordEnabled; WakePhrase.Text=_audioStore.Current.WakePhrase; TtsEnabled.IsChecked=_audioStore.Current.TtsEnabled; VoiceRate.Value=_audioStore.Current.TtsRate; VoiceVolume.Value=_audioStore.Current.TtsVolume;
        AiProvider.SelectedItem=AiProvider.Items.OfType<ComboBoxItem>().FirstOrDefault(x=>string.Equals(x.Content?.ToString(),_providerSettings.Provider,StringComparison.OrdinalIgnoreCase))??AiProvider.Items[0]; RukaEndpoint.Text=_providerSettings.RukaEndpoint; AiEndpoint.Text=_providerSettings.Endpoint; AiApiKey.Password=_providerSettings.ApiKey; AiModel.Text=_providerSettings.Model; SendHistory.IsChecked=_providerSettings.SendRecentHistory; HistoryCount.Value=_providerSettings.HistoryCount;
        DangerousConfirmation.IsChecked=_permissions.RequireConfirmationForDangerousActions; AllowScreenRead.IsChecked=_permissions.AllowScreenRead; AllowLaunchApps.IsChecked=_permissions.AllowLaunchApps; AllowCloseApps.IsChecked=_permissions.AllowCloseApps; AllowFileRead.IsChecked=_permissions.AllowFileRead; AllowFileWrite.IsChecked=_permissions.AllowFileWrite; AllowBrowserControl.IsChecked=_permissions.AllowBrowserControl; AllowSystemSettings.IsChecked=_permissions.AllowSystemSettings; NotificationsEnabled.IsChecked=_settings.NotificationsEnabled;
    }
    private void UpdateAppearanceLabels(){if(ScaleValue is null)return;ScaleValue.Text=$"サイズ: {ScaleSlider.Value:0.0}x";OpacityValue.Text=$"透明度: {OpacitySlider.Value:P0}";}
    private void Save_Click(object sender,RoutedEventArgs e)
    {
        _settings.StartWithWindows=StartWithWindows.IsChecked==true;_settings.AutonomousBehaviorEnabled=AutonomousBehavior.IsChecked==true;_settings.ShowDuringGames=ShowDuringGames.IsChecked==true;_settings.DiscordWakeWordOnly=DiscordWakeWordOnly.IsChecked==true;_settings.VoiceInputEnabled=VoiceInputEnabled.IsChecked==true;_settings.NotificationsEnabled=NotificationsEnabled.IsChecked==true;_settings.ActivePersonalityName=PersonalitySelector.SelectedItem?.ToString() ?? _settings.ActivePersonalityName;_settings.Save();
        _audioStore.Current.WakeWordEnabled=WakeEnabled.IsChecked==true;_audioStore.Current.WakePhrase=string.IsNullOrWhiteSpace(WakePhrase.Text)?"ねぇ、るか":WakePhrase.Text.Trim();_audioStore.Current.TtsEnabled=TtsEnabled.IsChecked==true;_audioStore.Current.TtsRate=(int)Math.Round(VoiceRate.Value);_audioStore.Current.TtsVolume=(int)Math.Round(VoiceVolume.Value);_audioStore.Save();
        _providerSettings.Provider=(AiProvider.SelectedItem as ComboBoxItem)?.Content?.ToString()??"ruka";_providerSettings.RukaEndpoint=string.IsNullOrWhiteSpace(RukaEndpoint.Text)?"https://ruka-ai.example.com/v1/chat":RukaEndpoint.Text.Trim();_providerSettings.Endpoint=AiEndpoint.Text.Trim();_providerSettings.ApiKey=AiApiKey.Password.Trim();_providerSettings.Model=string.IsNullOrWhiteSpace(AiModel.Text)?"gpt-4o-mini":AiModel.Text.Trim();_providerSettings.SendRecentHistory=SendHistory.IsChecked==true;_providerSettings.HistoryCount=(int)Math.Round(HistoryCount.Value);_providerSettings.Save();
        _permissions.RequireConfirmationForDangerousActions=DangerousConfirmation.IsChecked==true;_permissions.AllowScreenRead=AllowScreenRead.IsChecked==true;_permissions.AllowLaunchApps=AllowLaunchApps.IsChecked==true;_permissions.AllowCloseApps=AllowCloseApps.IsChecked==true;_permissions.AllowFileRead=AllowFileRead.IsChecked==true;_permissions.AllowFileWrite=AllowFileWrite.IsChecked==true;_permissions.AllowBrowserControl=AllowBrowserControl.IsChecked==true;_permissions.AllowSystemSettings=AllowSystemSettings.IsChecked==true;_permissions.Save();
        _appearance.Scale=ScaleSlider.Value;_appearance.Opacity=OpacitySlider.Value;_appearance.AlwaysOnTop=AlwaysOnTop.IsChecked==true;_appearance.ShowSpeechBubble=ShowBubble.IsChecked==true;_appearance.Save();
        System.Windows.MessageBox.Show("設定を保存したよ。","るか");
        SettingsSaved?.Invoke();
    }
    private void AiProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var kind = (AiProvider.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ruka";
        var useOwnApi = !kind.Equals("ruka", StringComparison.OrdinalIgnoreCase) && !kind.Equals("local", StringComparison.OrdinalIgnoreCase);
        AiApiKey.IsEnabled = useOwnApi;
        AiEndpoint.IsEnabled = useOwnApi;
        AiModel.IsEnabled = useOwnApi;
        RukaEndpoint.IsEnabled = kind.Equals("ruka", StringComparison.OrdinalIgnoreCase);
    }

    private async void TestAi_Click(object sender,RoutedEventArgs e)
    {
        try{var kind=(AiProvider.SelectedItem as ComboBoxItem)?.Content?.ToString()??"local";if(kind=="local"){System.Windows.MessageBox.Show("localモードは外部接続なしだよ。","るか");return;}if(kind=="ruka"){if(!Uri.TryCreate(RukaEndpoint.Text.Trim(),UriKind.Absolute,out var rukaEp))throw new InvalidOperationException("Ruka AIエンドポイントが正しくありません。");using var rukaClient=new HttpClient{Timeout=TimeSpan.FromSeconds(20)};var test=new RukaCloudProvider(rukaClient,rukaEp.ToString(),"接続テスト用。短く返答してください。",_providerSettings.InstallationId);var reply=await test.GenerateAsync("接続テスト。『接続OK』と短く返答してください。",[]);System.Windows.MessageBox.Show("Ruka AIサーバーから返答を受信したよ。\\n\\n"+reply,"Ruka AI接続成功");return;}if(!Uri.TryCreate(AiEndpoint.Text.Trim(),UriKind.Absolute,out var ep))throw new InvalidOperationException("エンドポイントが正しくありません。");var key=string.IsNullOrWhiteSpace(AiApiKey.Password)?Environment.GetEnvironmentVariable("RUKA_AI_API_KEY")??"":AiApiKey.Password.Trim();using var client=new HttpClient{Timeout=TimeSpan.FromSeconds(20)};IAiProvider test=kind=="openai"?new OpenAiCompatibleProvider(client,ep.ToString(),key, AiModel.Text.Trim(),"接続テスト用。短く返答してください。"):new HttpAiProvider(client,ep.ToString(),10);var reply=await test.GenerateAsync("接続テスト。『接続OK』と短く返答してください。",[]);System.Windows.MessageBox.Show("実際のサーバーから返答を受信したよ。\\n\\n"+reply,"AI接続成功");}catch(Exception ex){System.Windows.MessageBox.Show("実接続テスト失敗。\\n"+ex.Message,"るか",MessageBoxButton.OK,MessageBoxImage.Warning);}
    }
    private void TestNotification_Click(object sender, RoutedEventArgs e)
    {
        var notifications = new NotificationService { Enabled = true };
        notifications.Notify("るか", "通知テスト成功！デスクトップからちゃんと呼べたよ。");
    }

    private void Navigation_SelectionChanged(object sender,SelectionChangedEventArgs e)
    {
        if(Navigation.SelectedItem is not ListBoxItem item)return;var title=item.Content?.ToString()??"設定";PageTitle.Text=title;PageDescription.Text=title switch{"システム"=>"起動、自律行動、ゲーム時の基本動作を管理します。","外観"=>"サイズ、透明度、最前面、吹き出しを管理します。","会話"=>"会話履歴を長期記憶とは別に管理します。","音声"=>"マイク、ウェイクワード、TTSを管理します。","AI"=>"実際にAPIへ接続して返答できるAIを設定します。","記憶"=>"明示的に保存した長期記憶だけを管理します。","PC操作"=>"るかが実行できるPC操作と安全確認を管理します。","ゲーム"=>"ゲームごとの表示、退避、音声、サイズを管理します。","モニター"=>"モニターごとの有効化、位置、サイズを管理します。","通知"=>"通知設定を管理します。","キャラクター"=>"キャラクターアセットと性格プリセットを管理します。","権限"=>"PC操作の権限を個別に管理します。","診断"=>"るかが実際に動ける状態かをセルフチェックします。",_=>"ショートカットを確認します。"};foreach(var p in Pages.Children.OfType<StackPanel>())p.Visibility=Visibility.Collapsed;var page=title switch{"システム"=>SystemPage,"外観"=>AppearancePage,"会話"=>ConversationPage,"音声"=>VoicePage,"AI"=>AiPage,"記憶"=>MemoryPage,"PC操作"=>PcPage,"ゲーム"=>GamePage,"モニター"=>MonitorPage,"通知"=>NotificationPage,"キャラクター"=>CharacterPage,"権限"=>PermissionPage,"診断"=>DiagnosticsPage,_=>ShortcutPage};page.Visibility=Visibility.Visible;
    }
    private void RefreshMemory(){MemoryList.ItemsSource=_memory.Memories.ToList();}
    private void AddMemory_Click(object sender,RoutedEventArgs e){if(!string.IsNullOrWhiteSpace(MemoryInput.Text)){_memory.Remember(MemoryInput.Text);MemoryInput.Clear();RefreshMemory();}}
    private void ClearMemory_Click(object sender,RoutedEventArgs e){if(System.Windows.MessageBox.Show("長期記憶をすべて削除する？","確認",MessageBoxButton.YesNo)==MessageBoxResult.Yes){_memory.Clear();RefreshMemory();}}
    private void ClearConversation_Click(object sender,RoutedEventArgs e){if(System.Windows.MessageBox.Show("会話履歴をすべて削除する？","確認",MessageBoxButton.YesNo)==MessageBoxResult.Yes)_conversation.Clear();}
    private void LoadGameNames(){foreach(var name in _gameNames)GameSelector.Items.Add(name);GameSelector.SelectedIndex=0;}
    private void GameSelector_SelectionChanged(object sender,SelectionChangedEventArgs e){if(GameSelector.SelectedItem is not string name)return;var p=_games.Get(name);GameShow.IsChecked=p.ShowCharacter;GameMoveSide.IsChecked=p.MoveToSide;GameVoice.IsChecked=p.VoiceEnabled;GameScale.Value=p.Scale;}
    private void SaveGame_Click(object sender,RoutedEventArgs e){if(GameSelector.SelectedItem is not string name)return;_games.Set(name,new GameProfile(GameShow.IsChecked==true,GameScale.Value,"side",GameVoice.IsChecked==true,GameMoveSide.IsChecked==true));System.Windows.MessageBox.Show("ゲーム設定を保存したよ。","るか");}
    private void LoadPersonalities(){PersonalitySelector.ItemsSource=_personalities.Profiles.Select(x=>x.Name).ToList();if(_personalities.Profiles.Count>0){var index=_personalities.Profiles.ToList().FindIndex(x=>x.Name.Equals(_settings.ActivePersonalityName,StringComparison.OrdinalIgnoreCase));PersonalitySelector.SelectedIndex=index>=0?index:0;}}
    private void PersonalitySelector_SelectionChanged(object sender,SelectionChangedEventArgs e){var name=PersonalitySelector.SelectedItem?.ToString();var p=_personalities.Profiles.FirstOrDefault(x=>x.Name==name);if(p is null)return;PersonalityName.Text=p.Name;PersonalityPrompt.Text=p.SystemPrompt;PersonalityFirstPerson.Text=p.FirstPerson;PersonalityStyle.Text=p.SpeechStyle;}
    private void SavePersonality_Click(object sender,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(PersonalityName.Text))return;_personalities.Add(new PersonalityProfile(PersonalityName.Text.Trim(),PersonalityPrompt.Text.Trim(),string.IsNullOrWhiteSpace(PersonalityFirstPerson.Text)?"私":PersonalityFirstPerson.Text.Trim(),string.IsNullOrWhiteSpace(PersonalityStyle.Text)?"自然":PersonalityStyle.Text.Trim()));LoadPersonalities();System.Windows.MessageBox.Show("性格プリセットを保存したよ。","るか");}

    private void RunDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        var lines = new List<string>();
        try
        {
            lines.Add("[OK] 設定ファイル: 読み込み済み");
            lines.Add($"[OK] AIプロバイダー: {_providerSettings.Provider}");
            lines.Add(string.IsNullOrWhiteSpace(_providerSettings.EffectiveApiKey) ? "[WARN] AI APIキー: 未設定（local/http構成によっては不要）" : "[OK] AI APIキー: 設定済み");
            lines.Add($"[OK] AIモデル: {_providerSettings.Model}");
            lines.Add($"[OK] 音声入力: {_settings.VoiceInputEnabled}");
            lines.Add($"[OK] ウェイクワード: {_audioStore.Current.WakePhrase}");
            lines.Add($"[OK] 自律行動: {_settings.AutonomousBehaviorEnabled}");
            lines.Add($"[OK] 危険操作確認: {_permissions.RequireConfirmationForDangerousActions}");
            lines.Add($"[OK] PC操作権限: 起動={_permissions.AllowLaunchApps}, 終了={_permissions.AllowCloseApps}, ファイル読取={_permissions.AllowFileRead}, ブラウザ={_permissions.AllowBrowserControl}");
            var monitors = MonitorService.GetMonitors();
            lines.Add($"[OK] モニター: {monitors.Count}台");
            var assets = new CharacterAssetService();
            lines.Add(assets.TryLoad("ruka-idle.png") is null ? "[WARN] ruka-idle.png: 未配置（ベクターフォールバックを使用）" : "[OK] ruka-idle.png: 検出");
            lines.Add("[INFO] 詳細なAI接続確認は『AI』ページの実接続テストを使用してください。");
        }
        catch (Exception ex) { lines.Add("[ERROR] 診断中に例外: " + ex.Message); }
        DiagnosticsOutput.Text = string.Join(Environment.NewLine, lines);
    }

    private void LoadMonitors(){MonitorSelector.ItemsSource=MonitorService.GetMonitors().Select(x=>x.Id+(x.Primary?" (メイン)":"")).ToList();if(MonitorSelector.Items.Count>0)MonitorSelector.SelectedIndex=0;}
    private MonitorInfo? SelectedMonitor(){var list=MonitorService.GetMonitors();if(MonitorSelector.SelectedIndex<0||MonitorSelector.SelectedIndex>=list.Count)return null;return list[MonitorSelector.SelectedIndex];}
    private void MonitorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e){var m=SelectedMonitor();if(m is null)return;var p=_monitors.Get(m.Id);MonitorEnabled.IsChecked=p.Enabled;MonitorPosition.SelectedIndex=MonitorPosition.Items.OfType<ComboBoxItem>().ToList().FindIndex(x=>x.Content?.ToString()==p.Position);if(MonitorPosition.SelectedIndex<0)MonitorPosition.SelectedIndex=0;MonitorScale.Value=p.Scale;}
    private void SaveMonitor_Click(object sender,RoutedEventArgs e){var m=SelectedMonitor();if(m is null)return;var pos=(MonitorPosition.SelectedItem as ComboBoxItem)?.Content?.ToString()??"bottom-right";_monitors.Set(new MonitorProfile(m.Id,MonitorEnabled.IsChecked==true,pos,MonitorScale.Value));System.Windows.MessageBox.Show("モニター設定を保存したよ。","るか");}
}
