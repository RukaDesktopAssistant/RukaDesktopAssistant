using System.Diagnostics;
using System.Text;

namespace RukaDesktopAssistant.Services;

public sealed class PcActionService
{
    private readonly PermissionManager _permissions;
    private readonly SafeActionService _safe;

    public PcActionService(PermissionManager permissions)
    {
        _permissions = permissions;
        _safe = new SafeActionService(permissions);
    }

    public async Task<string?> TryHandleAsync(string text, Func<string, Task<bool>> confirm)
    {
        var t = text.Trim();

        if (t.StartsWith("アプリを開いて", StringComparison.OrdinalIgnoreCase) ||
            t.StartsWith("アプリ開いて", StringComparison.OrdinalIgnoreCase))
        {
            var target = t.Replace("アプリを開いて", "", StringComparison.OrdinalIgnoreCase)
                          .Replace("アプリ開いて", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (target.Length == 0) return "開きたいアプリ名を教えてね。";
            if (!_permissions.AllowLaunchApps) return "アプリ起動の権限がオフになってるよ。設定から許可してね。";
            try
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                return $"{target}を開いたよ。";
            }
            catch { return $"{target}を開けなかったよ。"; }
        }

        if (t.StartsWith("ブラウザで", StringComparison.OrdinalIgnoreCase) ||
            t.StartsWith("サイトを開いて", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowBrowserControl) return "ブラウザ操作の権限がオフだよ。";
            var url = t.StartsWith("ブラウザで", StringComparison.OrdinalIgnoreCase)
                ? t["ブラウザで".Length..].Trim()
                : t["サイトを開いて".Length..].Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return "http:// または https:// から始まるURLを指定してね。";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return "ブラウザで開いたよ。";
        }

        if (t.StartsWith("設定を開いて", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowSystemSettings) return "Windows設定の操作権限がオフだよ。";
            var uri = t["設定を開いて".Length..].Trim();
            var target = uri.Length == 0 ? "ms-settings:" : uri.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase) ? uri : "ms-settings:" + uri;
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            return "Windows設定を開いたよ。";
        }

        if (t.StartsWith("ファイルを探して", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowFileRead) return "ファイル読み取り権限がオフだよ。";
            var name = t["ファイルを探して".Length..].Trim();
            if (name.Length == 0) return "探したいファイル名を教えてね。";
            var roots = new[] { Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Environment.GetFolderPath(Environment.SpecialFolder.Downloads) };
            var found = new List<string>();
            foreach (var root in roots.Where(Directory.Exists))
            {
                try { found.AddRange(Directory.EnumerateFiles(root, "*" + name + "*", SearchOption.AllDirectories).Take(10)); } catch { }
            }
            return found.Count == 0 ? "見つからなかったよ。" : "見つけたよ：\n" + string.Join("\n", found);
        }

        if (t.Equals("シャットダウン", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("PCをシャットダウン", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowSystemSettings) return "システム操作の権限がオフだよ。";
            var ok = await _safe.TryExecuteAsync(
                new ActionRequest("shutdown", "PCをシャットダウンします", ActionRisk.High,
                    () => { Process.Start(new ProcessStartInfo("shutdown", "/s /t 0") { CreateNoWindow = true, UseShellExecute = false }); return Task.CompletedTask; }),
                confirm);
            return ok ? "シャットダウンを実行するね。" : "シャットダウンはキャンセルしたよ。";
        }

        return null;
    }
}
