using System.Speech.Synthesis;

namespace RukaDesktopAssistant.Services;

public sealed class VoiceService : IDisposable
{
    private readonly SpeechSynthesizer _synthesizer = new();

    public bool Enabled { get; set; } = true;
    public int Rate { get; set; } = 0;
    public int Volume { get; set; } = 100;

    public void Speak(string text)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(text)) return;
        _synthesizer.Rate = Math.Clamp(Rate, -10, 10);
        _synthesizer.Volume = Math.Clamp(Volume, 0, 100);
        _synthesizer.SpeakAsyncCancelAll();
        _synthesizer.SpeakAsync(text);
    }

    public void Stop() => _synthesizer.SpeakAsyncCancelAll();
    public void Dispose() => _synthesizer.Dispose();
}
