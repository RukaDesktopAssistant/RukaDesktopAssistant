using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RukaDesktopAssistant.Services.AI;

public sealed class OpenAiCompatibleProvider : IAiProvider
{
    private readonly HttpClient _client;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _systemPrompt;
    private readonly int _historyCount;
    private readonly bool _sendRecentHistory;

    public OpenAiCompatibleProvider(HttpClient client, string endpoint, string apiKey, string model, string systemPrompt, int historyCount = 20, bool sendRecentHistory = true)
    {
        _client = client;
        _endpoint = endpoint;
        _apiKey = apiKey;
        _model = model;
        _systemPrompt = systemPrompt;
        _historyCount = Math.Clamp(historyCount, 1, 100);
        _sendRecentHistory = sendRecentHistory;
    }

    public async Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("AI APIキーが設定されていません。設定画面でAPIキーを設定するか、RUKA_AI_API_KEY環境変数を設定してください。");

        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

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

        var payload = JsonSerializer.Serialize(new { model = _model, messages, temperature = 0.8 });
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await _client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"AIサーバーが {(int)response.StatusCode} を返しました: {body}");

        using var json = JsonDocument.Parse(body);
        var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("AIサーバーから空の返答が返りました。");
        return content.Trim();
    }
}