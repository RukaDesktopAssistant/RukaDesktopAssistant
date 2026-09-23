using System.Windows;

namespace RukaDesktopAssistant;

public partial class SettingsWindow : Window
{
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
            "権限" => "アプリごと・機能ごとに細かく許可範囲を設定します。",
            "記憶" => "会話履歴と長期記憶を分離して管理します。長期記憶は明示的な依頼がある場合だけ保存します。",
            "ゲーム" => "ゲームごと・モニターごとの表示、位置、反応を設定します。",
            "音声" => "ウェイクワード、音声認識、読み上げを設定します。",
            _ => "るかの動作をWindows設定のように管理します。"
        };
    }
}
