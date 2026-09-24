namespace RukaDesktopAssistant.Services.AI;

public sealed class LocalAiProvider : IAiProvider
{
    public Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default)
    {
        var text = userText.Trim();
        var previous = recentHistory.Reverse().FirstOrDefault(x => x.StartsWith("user:", StringComparison.OrdinalIgnoreCase));
        var last = previous is null ? "" : previous[(previous.IndexOf(':') + 1)..].Trim();

        string answer = text switch
        {
            var x when x.Contains("おはよう", StringComparison.OrdinalIgnoreCase) => "おはよう〜。今日はどうする？",
            var x when x.Contains("こんばんは", StringComparison.OrdinalIgnoreCase) => "こんばんは。今日もおつかれさま。",
            var x when x.Contains("ありがとう", StringComparison.OrdinalIgnoreCase) => "えへへ、どういたしまして。",
            var x when x.Contains("疲れ", StringComparison.OrdinalIgnoreCase) || x.Contains("つかれ", StringComparison.OrdinalIgnoreCase) => "おつかれさま。ちょっと休憩する？",
            var x when x.Contains("眠", StringComparison.OrdinalIgnoreCase) => "眠そうだね。無理しすぎないでね。",
            var x when x.Contains("元気", StringComparison.OrdinalIgnoreCase) => "元気だよ〜。そっちはどう？",
            var x when x.Contains("るか", StringComparison.OrdinalIgnoreCase) => "ん？どうした？",
            _ when !string.IsNullOrWhiteSpace(last) && !last.Equals(text, StringComparison.OrdinalIgnoreCase) => $"さっきの話の続きだね。もう少し聞かせて？",
            _ => $"うん、聞いてるよ。「{text}」について話そっか。"
        };
        return Task.FromResult(answer);
    }
}