namespace RukaDesktopAssistant.Services.AI;

public sealed class LocalAiProvider : IAiProvider
{
    public Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default)
    {
        var answer = userText.Contains("るか", StringComparison.OrdinalIgnoreCase)
            ? "ん？どうした？"
            : "受け取ったよ。AIプロバイダーを接続すると、ここから本格的な会話ができるよ。";
        return Task.FromResult(answer);
    }
}
