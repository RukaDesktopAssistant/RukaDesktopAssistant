using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("OpenAI", client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
});

var app = builder.Build();

var apiKey = Environment.GetEnvironmentVariable("RUKA_OPENAI_API_KEY") ?? "";
var model = Environment.GetEnvironmentVariable("RUKA_AI_MODEL") ?? "gpt-4o-mini";
var maxRequestsPerMinute = int.TryParse(Environment.GetEnvironmentVariable("RUKA_RATE_LIMIT"), out var limit)
    ? Math.Clamp(limit, 1, 120)
    : 30;

var buckets = new Dictionary<string, RateBucket>(StringComparer.Ordinal);
var sync = new object();

app.MapGet("/health", () => Results.Ok(new { ok = true, service = "ruka-ai" }));

app.MapPost("/v1/chat", async (HttpContext context, ChatRequest request, IHttpClientFactory clients, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Problem("Ruka AI server is not configured.", statusCode: 503);

    if (request.Messages is null || request.Messages.Count == 0)
        return Results.BadRequest(new { error = "messages is required" });

    var installationId = context.Request.Headers["X-Ruka-Installation"].ToString().Trim();
    if (installationId.Length < 8 || installationId.Length > 128)
        return Results.BadRequest(new { error = "X-Ruka-Installation is required" });

    var remote = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var bucketKey = installationId + "|" + remote;

    lock (sync)
    {
        var now = DateTimeOffset.UtcNow;
        if (!buckets.TryGetValue(bucketKey, out var bucket) ||
            now - bucket.WindowStart >= TimeSpan.FromMinutes(1))
        {
            bucket = new RateBucket(now, 0);
            buckets[bucketKey] = bucket;
        }

        if (bucket.Count >= maxRequestsPerMinute)
            return Results.StatusCode((int)HttpStatusCode.TooManyRequests);

        buckets[bucketKey] = bucket with { Count = bucket.Count + 1 };
    }

    var messages = request.Messages
        .Take(42)
        .Select(x => new
        {
            role = NormalizeRole(x.Role),
            content = (x.Content ?? "").Length > 8000 ? x.Content[..8000] : x.Content
        })
        .Where(x => !string.IsNullOrWhiteSpace(x.content))
        .ToList();

    if (messages.Count == 0)
        return Results.BadRequest(new { error = "messages are empty" });

    var payload = JsonSerializer.Serialize(new
    {
        model,
        messages,
        temperature = 0.8
    });

    var client = clients.CreateClient("OpenAI");
    using var outgoing = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
    outgoing.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    outgoing.Content = new StringContent(payload, Encoding.UTF8, "application/json");

    using var response = await client.SendAsync(outgoing, ct);
    var body = await response.Content.ReadAsStringAsync(ct);

    if (!response.IsSuccessStatusCode)
        return Results.Content(body, "application/json", Encoding.UTF8, (int)response.StatusCode);

    return Results.Content(body, "application/json", Encoding.UTF8);
});

app.Run();

static string NormalizeRole(string? role) =>
    role?.ToLowerInvariant() switch
    {
        "system" => "system",
        "developer" => "developer",
        "assistant" => "assistant",
        _ => "user"
    };

record ChatRequest(List<ChatMessage>? Messages);
record ChatMessage(string? Role, string? Content);
record RateBucket(DateTimeOffset WindowStart, int Count);
