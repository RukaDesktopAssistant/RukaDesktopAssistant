using System.Windows;
using RukaDesktopAssistant.Services;

namespace RukaDesktopAssistant;

public partial class SettingsWindow : Window
{
    private readonly PermissionManager _permissions = new();
    private readonly AudioSettings _audio = new();
    private readonly WakeWordService _wake = new();

    public SettingsWindow()
    {
        InitializeComponent();
        Navigation.SelectedIndex = 0;
    }

    private void Navigation_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (PageTitle is null || Navigation.SelectedItem is not System.Windows.Controls.ListBoxItem item) return;
        var title = item.Content?.ToString() ?? "設定";
        PageTitle.Text = title;
        PageDescription.Text = title switch
        {
            "PC操作" => "るかがPCを操作できる範囲を管理します。危険な操作は別途確認します。",
            "権限" => $"画面読み取り: {_permissions.AllowScreenRead} / アプリ起動: {_permissions.AllowLaunchApps} / ファイル書込: {_permissions.AllowFileWrite}",
            "記憶" => "会話履歴と長期記憶を分離して管理します。長期記憶は明示的な依頼がある場合だけ保存します。",
            "ゲーム" => "ゲームごと・モニターごとの表示、位置、反応を設定します。",
            "音声" => $"ウェイクワード: {_wake.WakePhrase} / 音声合成: {_audio.TtsEnabled}",
            "通知" => "Windows通知との統合を管理します。",
            _ => "るかの動作をWindows設定のように管理します。"
        };
    }
}
