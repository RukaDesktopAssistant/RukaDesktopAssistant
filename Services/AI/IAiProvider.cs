namespace RukaDesktopAssistant.Services.AI;

public interface IAiProvider
{
    Task<string> GenerateAsync(string userText, IReadOnlyList<string> recentHistory, CancellationToken cancellationToken = default);
}
