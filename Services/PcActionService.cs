using System.Diagnostics;

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

        if (t.StartsWith("アプリを開いて", StringComparison.OrdinalIgnoreCase) || t.StartsWith("アプリ開いて", StringComparison.OrdinalIgnoreCase))
        {
            var target = t.Replace("アプリを開いて", "", StringComparison.OrdinalIgnoreCase).Replace("アプリ開いて", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (target.Length == 0) return "開きたいアプリ名を教えてね。";
            if (!_permissions.AllowLaunchApps) return "アプリ起動の権限がオフになってるよ。設定から許可してね。";
            try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); return $"{target}を開いたよ。"; }
            catch { return $"{target}を開けなかったよ。"; }
        }

        if (t.StartsWith("ブラウザで", StringComparison.OrdinalIgnoreCase) || t.StartsWith("サイトを開いて", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowBrowserControl) return "ブラウザ操作の権限がオフだよ。";
            var url = t.StartsWith("ブラウザで", StringComparison.OrdinalIgnoreCase) ? t["ブラウザで".Length..].Trim() : t["サイトを開いて".Length..].Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return "http:// または https:// のURLを指定してね。";
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
            return "ブラウザで開いたよ。";
        }

        if (t.StartsWith("設定を開いて", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowSystemSettings) return "Windows設定の操作権限がオフだよ。";
            var uri = t["設定を開いて".Length..].Trim();
            var target = uri.Length == 0 ? "ms-settings:" : uri.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase) ? uri : "ms-settings:" + uri;
            try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); return "Windows設定を開いたよ。"; }
            catch { return "Windows設定を開けなかったよ。"; }
        }

        if (t.StartsWith("ファイルに書いて", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowFileWrite) return "ファイル書き込み権限がオフだよ。";
            var payload = t["ファイルに書いて".Length..].Trim();
            var parts = payload.Split('|', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0])) return "「ファイルに書いて ファイル名 | 内容」の形式で指定してね。";
            var path = parts[0];
            var allowedRoots = new[] { Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Environment.GetFolderPath(Environment.SpecialFolder.Downloads) }.Where(Directory.Exists).Select(Path.GetFullPath).ToArray();
            try
            {
                var full = Path.GetFullPath(path);
                if (!allowedRoots.Any(root => full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || string.Equals(full, root, StringComparison.OrdinalIgnoreCase))) return "安全のため、デスクトップ・ドキュメント・ダウンロード内だけに書き込めるよ。";
                var ok = await _safe.TryExecuteAsync(new ActionRequest("write-file", $"ファイルを書き込みます: {full}", ActionRisk.Medium, () => { Directory.CreateDirectory(Path.GetDirectoryName(full)!); File.WriteAllText(full, parts[1]); return Task.CompletedTask; }), confirm);
                return ok ? $"ファイルを書き込んだよ: {full}" : "ファイル書き込みはキャンセルしたよ。";
            }
            catch (Exception ex) { return "ファイルを書き込めなかったよ: " + ex.Message; }
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
                try { found.AddRange(Directory.EnumerateFiles(root, "*" + name + "*", SearchOption.AllDirectories).Take(20)); } catch { }
            }
            return found.Count == 0 ? "見つからなかったよ。" : "見つけたよ：\n" + string.Join("\n", found);
        }

        if (t.StartsWith("アプリを閉じて", StringComparison.OrdinalIgnoreCase) || t.StartsWith("アプリ閉じて", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowCloseApps) return "アプリ終了の権限がオフだよ。";
            var target = t.Replace("アプリを閉じて", "", StringComparison.OrdinalIgnoreCase).Replace("アプリ閉じて", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (target.Length == 0) return "閉じたいアプリ名を教えてね。";
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(target));
            if (processes.Length == 0) return $"{target}は見つからなかったよ。";
            var ok = await _safe.TryExecuteAsync(
                new ActionRequest("close-app", $"{target}を終了します。", ActionRisk.High, () =>
                {
                    foreach (var p in processes) { try { if (!p.HasExited) p.CloseMainWindow(); } catch { } }
                    return Task.CompletedTask;
                }), confirm);
            return ok ? $"{target}の終了を実行したよ。" : "アプリ終了はキャンセルしたよ。";
        }

        if (t.Equals("シャットダウン", StringComparison.OrdinalIgnoreCase) || t.Equals("PCをシャットダウン", StringComparison.OrdinalIgnoreCase))
        {
            if (!_permissions.AllowSystemSettings) return "システム操作の権限がオフだよ。";
            var ok = await _safe.TryExecuteAsync(
                new ActionRequest("shutdown", "PCをシャットダウンします", ActionRisk.High, () => { Process.Start(new ProcessStartInfo("shutdown", "/s /t 0") { CreateNoWindow = true, UseShellExecute = false }); return Task.CompletedTask; }),
                confirm);
            return ok ? "シャットダウンを実行するね。" : "シャットダウンはキャンセルしたよ。";
        }

        return null;
    }
}