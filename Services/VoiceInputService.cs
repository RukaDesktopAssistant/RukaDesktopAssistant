using System.Globalization;
using System.Speech.Recognition;
using System.Windows.Threading;

namespace RukaDesktopAssistant.Services;

public sealed class VoiceInputService : IDisposable
{
    private readonly WakeWordService _wakeWord;
    private SpeechRecognitionEngine? _engine;
    private readonly DispatcherTimer _sessionTimer = new() { Interval = TimeSpan.FromSeconds(8) };
    private bool _conversationActive;

    public bool IsListening { get; private set; }
    public event Action<string>? Recognized;
    public event Action<Exception>? Error;

    public VoiceInputService(WakeWordService wakeWord)
    {
        _wakeWord = wakeWord;
        _sessionTimer.Tick += (_, _) => EndConversation();
    }

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
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!_conversationActive)
        {
            if (!_wakeWord.Matches(text)) return;

            _conversationActive = true;
            _sessionTimer.Stop();
            _sessionTimer.Start();

            var phrase = _wakeWord.RemoveWakePhrase(text);
            Recognized?.Invoke(string.IsNullOrWhiteSpace(phrase) ? "" : phrase);
            return;
        }

        _sessionTimer.Stop();
        _sessionTimer.Start();
        Recognized?.Invoke(text.Trim());
    }

    private void EndConversation()
    {
        _conversationActive = false;
        _sessionTimer.Stop();
    }

    public void StopConversation()
    {
        EndConversation();
        IsListening = false;
        try { _engine?.RecognizeAsyncStop(); } catch { }
    }

    public void Dispose()
    {
        _sessionTimer.Stop();
        try { _engine?.RecognizeAsyncCancel(); } catch { }
        _engine?.Dispose();
        _engine = null;
        IsListening = false;
        _conversationActive = false;
    }
}
