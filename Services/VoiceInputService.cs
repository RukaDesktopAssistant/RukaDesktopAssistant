namespace RukaDesktopAssistant.Services;

public sealed class VoiceInputService
{
    private readonly WakeWordService _wakeWord;
    public bool IsListening { get; private set; }
    public event Action<string>? Recognized;

    public VoiceInputService(WakeWordService wakeWord) => _wakeWord = wakeWord;

    public void FeedRecognizedText(string text)
    {
        if (!_wakeWord.Matches(text)) return;
        IsListening = true;
        Recognized?.Invoke(text[_wakeWord.WakePhrase.Length..].Trim());
    }

    public void StopConversation() => IsListening = false;
}
