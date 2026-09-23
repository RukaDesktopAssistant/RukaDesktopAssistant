namespace RukaDesktopAssistant.Services;

public sealed class ContextPolicy
{
    public bool SuppressDuringGames { get; set; } = true;
    public bool WakeWordOnlyDuringCalls { get; set; } = true;

    public bool ShouldSuppress(AppContextInfo context) => SuppressDuringGames && context.IsKnownGame;
}
