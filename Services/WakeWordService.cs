namespace RukaDesktopAssistant.Services;

public sealed class WakeWordService
{
    public string WakePhrase { get; set; } = "ねぇ、るか";
    public bool Enabled { get; set; } = true;

    public bool Matches(string recognizedText)
    {
        if (!Enabled) return false;
        return recognizedText.Contains(WakePhrase, StringComparison.OrdinalIgnoreCase);
    }
}
