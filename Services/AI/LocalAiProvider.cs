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

    public LocalAiProvider()
    {
        _patterns = LoadPatterns();
    }

    public Task<string> GenerateAsync(
        string userText,
        IReadOnlyList<string> recentHistory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var text = userText.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult("ん？どうした？");

        var answer = FindPattern(text, recentHistory);

        if (answer is null)
            answer = RuleBasedFallback(text, recentHistory);

        return Task.FromResult(answer);
    }

    private string? FindPattern(string text, IReadOnlyList<string> recentHistory)
    {
        var candidates = _patterns
            .Select(p => (Pattern: p, Score: Score(p, text, recentHistory)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Pattern.Id)
            .Take(8)
            .ToList();

        if (candidates.Count == 0)
            return null;

        // Prefer the strongest local match, while allowing a small amount
        // of context/history information to influence the score.
        return candidates[0].Pattern.Response;
    }

    private static double Score(
        LocalPattern pattern,
        string text,
        IReadOnlyList<string> recentHistory)
    {
        var trigger = pattern.Trigger.Trim();
        if (string.IsNullOrEmpty(trigger))
            return 0;

        var score = 0d;

        if (string.Equals(text, trigger, StringComparison.OrdinalIgnoreCase))
            score += 100 * pattern.Weight;
        else if (text.Contains(trigger, StringComparison.OrdinalIgnoreCase))
            score += 25 * pattern.Weight;
        else if (trigger.Contains(text, StringComparison.OrdinalIgnoreCase) && text.Length >= 2)
            score += 8 * pattern.Weight;

        if (score <= 0)
            return 0;

        if (!string.IsNullOrWhiteSpace(pattern.Context) &&
            recentHistory.Any(h => h.Contains(pattern.Context, StringComparison.OrdinalIgnoreCase)))
        {
            score += 2;
        }

        // Longer matching phrases are generally more specific than tiny keywords.
        score += Math.Min(trigger.Length, 20) * 0.25;
        return score;
    }

    private static List<LocalPattern> LoadPatterns()
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            var path = Path.Combine(baseDirectory, "Data", "ruka-conversation-seed-v5-large.json");

            if (!File.Exists(path))
                return [];

            var json = File.ReadAllText(path);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("entries", out var entries))
                return [];

            var result = new List<LocalPattern>();

            foreach (var item in entries.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idValue) ? idValue.GetString() : null;
                var category = item.TryGetProperty("category", out var categoryValue) ? categoryValue.GetString() : null;
                var trigger = item.TryGetProperty("trigger", out var triggerValue) ? triggerValue.GetString() : null;
                var response = item.TryGetProperty("response", out var responseValue) ? responseValue.GetString() : null;
                var context = item.TryGetProperty("context", out var contextValue) ? contextValue.GetString() : null;
                var weight = item.TryGetProperty("weight", out var weightValue) && weightValue.TryGetDouble(out var parsedWeight)
                    ? parsedWeight
                    : 1d;

                if (!string.IsNullOrWhiteSpace(id) &&
                    !string.IsNullOrWhiteSpace(trigger) &&
                    !string.IsNullOrWhiteSpace(response))
                {
                    result.Add(new LocalPattern(id, category ?? "general", trigger, response, context, weight));
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
            var x when x.Contains("おはよう", StringComparison.OrdinalIgnoreCase) => "おはよう〜。今日はどうする？",
            var x when x.Contains("こんばんは", StringComparison.OrdinalIgnoreCase) => "こんばんは。今日もおつかれさま。",
            var x when x.Contains("ありがとう", StringComparison.OrdinalIgnoreCase) => "えへへ、どういたしまして。",
            var x when x.Contains("疲れ", StringComparison.OrdinalIgnoreCase) || x.Contains("つかれ", StringComparison.OrdinalIgnoreCase) => "おつかれさま。ちょっと休憩する？",
            var x when x.Contains("眠", StringComparison.OrdinalIgnoreCase) => "眠そうだね。無理しすぎないでね。",
            var x when x.Contains("元気", StringComparison.OrdinalIgnoreCase) => "元気だよ〜。そっちはどう？",
            var x when x.Contains("るか", StringComparison.OrdinalIgnoreCase) => "ん？どうした？",
            _ when !string.IsNullOrWhiteSpace(last) && !last.Equals(text, StringComparison.OrdinalIgnoreCase) => "さっきの話の続きだね。もう少し聞かせて？",
            _ => $"うん、聞いてるよ。「{text}」について話そっか。"
        };
    }
}
