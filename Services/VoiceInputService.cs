using System.Globalization;
using System.Speech.Recognition;

namespace RukaDesktopAssistant.Services;

public sealed class VoiceInputService : IDisposable
{
    private readonly WakeWordService _wakeWord;
    private SpeechRecognitionEngine? _engine;

    public bool IsListening { get; private set; }
    public event Action<string>? Recognized;
    public event Action<Exception>? Error;

    public VoiceInputService(WakeWordService wakeWord) => _wakeWord = wakeWord;

    public bool Start()
    {
        if (IsListening) return true;

        try
        {
            _engine = new SpeechRecognitionEngine(new CultureInfo("ja-JP"));
            _engine.LoadGrammar(new DictationGrammar());
            _engine.SpeechRecognized += OnSpeechRecognized;
            _engine.SetInputToDefaultAudioDevice();
            _engine.RecognizeAsync(RecognizeMode.Multiple);
            IsListening = true;
            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(ex);
            Stop();
            return false;
        }
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        if (e.Result.Confidence < 0.45f) return;
        FeedRecognizedText(e.Result.Text);
    }

    public void FeedRecognizedText(string text)
    {
        if (!_wakeWord.Matches(text)) return;

        IsListening = true;
        var phrase = _wakeWord.RemoveWakePhrase(text);
        Recognized?.Invoke(phrase);
    }

    public void StopConversation()
    {
        IsListening = false;
        try { _engine?.RecognizeAsyncStop(); } catch { }
    }

    public void Dispose()
    {
        try { _engine?.RecognizeAsyncCancel(); } catch { }
        _engine?.Dispose();
        _engine = null;
        IsListening = false;
    }
}
