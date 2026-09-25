using System.Text.Json;

namespace RukaDesktopAssistant.Services.AI;

public sealed class LocalAiProvider : IAiProvider
{
    private sealed record LocalPattern(
        string Id,
        string Category,
        string Trigger,
        string Response,
        string? Context = null,
        double Weight = 1);

    private readonly List<LocalPattern> _patterns;
    private readonly Dictionary<string, List<LocalPattern>> _index;

    public LocalAiProvider()
    {
        _patterns = LoadPatterns();
        _index = BuildIndex(_patterns);
    }

    public Task<string> GenerateAsync(
        string userText,
        IReadOnlyList<string> recentHistory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var text = Normalize(userText);
        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult("ん？どうした？");

        var answer = FindPattern(text, recentHistory, cancellationToken)
            ?? RuleBasedFallback(text, recentHistory);

        return Task.FromResult(answer);
    }

    private string? FindPattern(
        string text,
        IReadOnlyList<string> recentHistory,
        CancellationToken cancellationToken)
    {
        var candidateSet = new HashSet<LocalPattern>();

        // Fast path: retrieve only patterns whose normalized trigger contains
        // at least one meaningful character/phrase from the user's message.
        foreach (var key in ExtractKeys(text))
        {
            if (!_index.TryGetValue(key, out var matches))
                continue;

            foreach (var pattern in matches)
                candidateSet.Add(pattern);
        }

        // For very short messages, the index can be too restrictive.
        // Scan the compact corpus only in that case.
        if (candidateSet.Count == 0 && text.Length <= 8)
            candidateSet.UnionWith(_patterns);

        var history = recentHistory
            .TakeLast(8)
            .Select(Normalize)
            .Where(x => x.Length > 0)
            .ToArray();

        return candidateSet
            .Select(p =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return (Pattern: p, Score: Score(p, text, history));
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Pattern.Trigger.Length)
            .ThenBy(x => x.Pattern.Id)
            .Take(6)
            .Select(x => x.Pattern.Response)
            .FirstOrDefault();
    }

    private static double Score(
        LocalPattern pattern,
        string text,
        IReadOnlyList<string> history)
    {
        var trigger = Normalize(pattern.Trigger);
        if (trigger.Length == 0)
            return 0;

        var score = 0d;

        if (text.Equals(trigger, StringComparison.Ordinal))
            score += 120;
        else if (text.Contains(trigger, StringComparison.Ordinal))
            score += 55 + Math.Min(trigger.Length, 30);
        else if (trigger.Contains(text, StringComparison.Ordinal) && text.Length >= 2)
            score += 12 + text.Length;

        var textKeys = ExtractKeys(text);
        var triggerKeys = ExtractKeys(trigger);

        if (textKeys.Count > 0 && triggerKeys.Count > 0)
        {
            var overlap = textKeys.Count(k => triggerKeys.Contains(k));
            score += overlap * 7;
        }

        if (history.Count > 0)
        {
            var categoryHits = history.Count(h =>
                h.Contains(Normalize(pattern.Category), StringComparison.Ordinal));

            if (categoryHits > 0)
                score += Math.Min(4, categoryHits);
        }

        if (!string.IsNullOrWhiteSpace(pattern.Context))
        {
            var context = Normalize(pattern.Context);
            if (history.Any(h => h.Contains(context, StringComparison.Ordinal)))
                score += 8;
        }

        score += Math.Min(trigger.Length, 24) * 0.35;
        return score * Math.Max(0.1, pattern.Weight);
    }

    private static HashSet<string> ExtractKeys(string text)
    {
        var normalized = Normalize(text);
        var keys = new HashSet<string>(StringComparer.Ordinal);

        if (normalized.Length >= 2)
        {
            for (var i = 0; i < normalized.Length - 1; i++)
            {
                var pair = normalized.Substring(i, 2);
                if (pair.All(IsUsefulCharacter))
                    keys.Add(pair);
            }
        }

        foreach (var keyword in new[]
        {
            "るか", "Ruka", "AI", "API", "GitHub", "Roblox", "Fortnite",
            "VALORANT", "Apex", "YouTube", "OBS", "WiFi", "ping", "FPS",
            "C#", "WPF", "EXE", "勉強", "数学", "英語", "理科", "化学",
            "歌", "音楽", "配信", "ゲーム", "履歴", "記憶", "オフライン"
        })
        {
            if (normalized.Contains(Normalize(keyword), StringComparison.Ordinal))
                keys.Add(Normalize(keyword));
        }

        return keys;
    }

    private static bool IsUsefulCharacter(char c) =>
        char.IsLetterOrDigit(c) || c >= 0x3040;

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim()
            .Replace("　", " ")
            .Replace("ｗ", "w")
            .Replace("Ｗ", "w")
            .ToLowerInvariant();
    }

    private static Dictionary<string, List<LocalPattern>> BuildIndex(IEnumerable<LocalPattern> patterns)
    {
        var index = new Dictionary<string, List<LocalPattern>>(StringComparer.Ordinal);

        foreach (var pattern in patterns)
        {
            foreach (var key in ExtractKeys(pattern.Trigger))
            {
                if (!index.TryGetValue(key, out var list))
                {
                    list = [];
                    index[key] = list;
                }

                list.Add(pattern);
            }
        }

        return index;
    }

    private static List<LocalPattern> LoadPatterns()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "ruka-conversation-seed-v5-large.json");
            if (!File.Exists(path))
                return [];

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (!document.RootElement.TryGetProperty("entries", out var entries))
                return [];

            var result = new List<LocalPattern>();

            foreach (var item in entries.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var a) ? a.GetString() : null;
                var category = item.TryGetProperty("category", out var b) ? b.GetString() : null;
                var trigger = item.TryGetProperty("trigger", out var c) ? c.GetString() : null;
                var response = item.TryGetProperty("response", out var d) ? d.GetString() : null;
                var context = item.TryGetProperty("context", out var e) ? e.GetString() : null;
                var weight = item.TryGetProperty("weight", out var f) && f.TryGetDouble(out var w) ? w : 1d;

                if (!string.IsNullOrWhiteSpace(id) &&
                    !string.IsNullOrWhiteSpace(trigger) &&
                    !string.IsNullOrWhiteSpace(response))
                {
                    result.Add(new LocalPattern(
                        id,
                        category ?? "general",
                        trigger,
                        response,
                        context,
                        weight));
                }
            }

            return result;
        }
        catch
        {
            return [];
        }
    }

    private static string RuleBasedFallback(string text, IReadOnlyList<string> recentHistory)
    {
        var previous = recentHistory
            .Reverse()
            .FirstOrDefault(x => x.StartsWith("user:", StringComparison.OrdinalIgnoreCase));

        var last = previous is null
            ? ""
            : previous[(previous.IndexOf(':') + 1)..].Trim();

        return text switch
        {
            var x when x.Contains("おはよう", StringComparison.Ordinal) => "おはよう〜。今日はどうする？",
            var x when x.Contains("こんばんは", StringComparison.Ordinal) => "こんばんは。今日もおつかれさま。",
            var x when x.Contains("ありがとう", StringComparison.Ordinal) => "えへへ、どういたしまして。",
            var x when x.Contains("疲れ", StringComparison.Ordinal) || x.Contains("つかれ", StringComparison.Ordinal) => "おつかれさま。ちょっと休憩する？",
            var x when x.Contains("眠", StringComparison.Ordinal) => "眠そうだね。無理しすぎないでね。",
            var x when x.Contains("元気", StringComparison.Ordinal) => "元気だよ〜。そっちはどう？",
            var x when x.Contains("るか", StringComparison.Ordinal) => "ん？どうした？",
            _ when !string.IsNullOrWhiteSpace(last) && !last.Equals(text, StringComparison.OrdinalIgnoreCase) => "さっきの話の続きだね。もう少し聞かせて？",
            _ => $"うん、聞いてるよ。「{text}」について話そっか。"
        };
    }
}
