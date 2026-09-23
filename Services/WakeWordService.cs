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

    public string RemoveWakePhrase(string recognizedText)
    {
        if (!Matches(recognizedText)) return recognizedText.Trim();
        return recognizedText.Replace(WakePhrase, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
    }
}
