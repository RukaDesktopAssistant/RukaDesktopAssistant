namespace RukaDesktopAssistant.Services.AI;

public sealed class FallbackAiProvider : IAiProvider
{
    private readonly IAiProvider _primary;
    private readonly IAiProvider _fallback;

    public FallbackAiProvider(IAiProvider primary, IAiProvider fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public async Task<string> GenerateAsync(
        string userText,
        IReadOnlyList<string> recentHistory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _primary.GenerateAsync(userText, recentHistory, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return await _fallback.GenerateAsync(userText, recentHistory, cancellationToken);
        }
    }
}
