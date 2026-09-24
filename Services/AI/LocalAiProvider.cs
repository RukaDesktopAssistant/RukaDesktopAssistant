namespace RukaDesktopAssistant.Services.AI;

public sealed class LocalAiProvider : IAiProvider
{
    public Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default)
    {
        var text = userText.Trim();
        var lastUser = recentHistory.Reverse().FirstOrDefault(x => x.StartsWith("user:", StringComparison.OrdinalIgnoreCase));
        var last = lastUser is null ? "" : lastUser[(lastUser.IndexOf(':') + 1)..].Trim();

        string answer;
        if (text.Contains("おはよう", StringComparison.OrdinalIgnoreCase)) answer = "おはよう〜。今日はどうする？";
        else if (text.Contains("こんばんは", StringComparison.OrdinalIgnoreCase)) answer = "こんばんは。今日もおつかれさま。";
        else if (text.Contains("ありがとう", StringComparison.OrdinalIgnoreCase)) answer = "えへへ、どういたしまして。";
        else if (text.Contains("疲れ", StringComparison.OrdinalIgnoreCase) || text.Contains("つかれ", StringComparison.OrdinalIgnoreCase)) answer = "おつかれさま。ちょっと休憩する？";
        else if (text.Contains("眠", StringComparison.OrdinalIgnoreCase)) answer = "眠そうだね。無理しすぎないでね。";
        else if (text.Contains("るか", StringComparison.OrdinalIgnoreCase)) answer = "ん？どうした？";
        else if (!string.IsNullOrEmpty(last) && last != text) answer = $"さっきの話の続きだね。「{last}」について、もう少し聞かせて？";
        else answer = $"うん、聞いてるよ。「{text}」についてもう少し教えて〜。";
        return Task.FromResult(answer);
    }
}