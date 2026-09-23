using System.Net.Http;
using System.Net.Http.Json;

namespace RukaDesktopAssistant.Services.AI;

public sealed class HttpAiProvider : IAiProvider
{
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public HttpAiProvider(HttpClient http, string endpoint)
    {
        _http = http;
        _endpoint = endpoint;
    }

    public async Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default)
    {
        var payload = new { message = userText, history = recentHistory.TakeLast(20).ToArray() };
        using var response = await _http.PostAsJsonAsync(_endpoint, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AiResponse>(cancellationToken: cancellationToken);
        return result?.Text ?? "返答を受け取れなかったよ。";
    }

    private sealed record AiResponse(string Text);
}
