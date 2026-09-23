using System.Net.Http;
using System.Net.Http.Json;

namespace RukaDesktopAssistant.Services.AI;

public sealed class HttpAiProvider : IAiProvider
{
    private readonly HttpClient _http;
    private readonly string _endpoint;
    private readonly int _historyCount;
    private readonly bool _sendRecentHistory;

    public HttpAiProvider(
        HttpClient http,
        string endpoint,
        int historyCount = 20,
        bool sendRecentHistory = true)
    {
        _http = http;
        _endpoint = endpoint;
        _historyCount = Math.Clamp(historyCount, 1, 100);
        _sendRecentHistory = sendRecentHistory;
    }

    public async Task<string> GenerateAsync(
        string userText,
        IReadOnlyList<string> recentHistory,
        CancellationToken cancellationToken = default)
    {
        var history = _sendRecentHistory
            ? recentHistory.TakeLast(_historyCount).ToArray()
            : Array.Empty<string>();

        var payload = new { message = userText, history };
        using var response = await _http.PostAsJsonAsync(_endpoint, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AiResponse>(
            cancellationToken: cancellationToken);

        return result?.Text ?? "返答を受け取れなかったよ。";
    }

    private sealed record AiResponse(string Text);
}
