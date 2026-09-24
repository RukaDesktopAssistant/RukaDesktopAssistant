using System.Text;
using System.Text.Json;

namespace RukaDesktopAssistant.Services.AI;

public sealed class RukaCloudProvider : IAiProvider
{
    private readonly HttpClient _client;
    private readonly string _endpoint;
    private readonly string _systemPrompt;
    private readonly int _historyCount;
    private readonly bool _sendRecentHistory;
    private readonly string _installationId;

    public RukaCloudProvider(HttpClient client, string endpoint, string systemPrompt, string installationId, int historyCount = 20, bool sendRecentHistory = true)
    {
        _client = client;
        _endpoint = endpoint;
        _systemPrompt = systemPrompt;
        _installationId = installationId;
        _historyCount = Math.Clamp(historyCount, 1, 100);
        _sendRecentHistory = sendRecentHistory;
    }

    public async Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default)
    {
        var messages = new List<object> { new { role = "system", content = _systemPrompt } };

        if (_sendRecentHistory)
        {
            foreach (var item in recentHistory.TakeLast(_historyCount))
            {
                var split = item.IndexOf(':');
                if (split <= 0) continue;
                var role = item[..split].Trim().Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user";
                messages.Add(new { role, content = item[(split + 1)..].Trim() });
            }
        }

        messages.Add(new { role = "user", content = userText });

        var payload = JsonSerializer.Serialize(new { messages });
        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        request.Headers.Add("X-Ruka-Installation", _installationId);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await _client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Ruka AIサーバーが {(int)response.StatusCode} を返しました。");

        using var json = JsonDocument.Parse(body);
        var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Ruka AIから空の返答が返りました。");

        return content.Trim();
    }
}
