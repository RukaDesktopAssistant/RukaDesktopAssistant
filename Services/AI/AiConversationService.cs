namespace RukaDesktopAssistant.Services.AI;

public sealed class AiConversationService
{
    private readonly IAiProvider _provider;
    public AiConversationService(IAiProvider provider) => _provider = provider;
    public Task<string> ReplyAsync(string text, IReadOnlyList<string> history, CancellationToken token = default) => _provider.GenerateAsync(text, history, token);
}
