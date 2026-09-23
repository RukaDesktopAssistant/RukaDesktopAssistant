namespace RukaDesktopAssistant.Services;

public sealed class AudioSettings
{
    public bool WakeWordEnabled { get; set; } = true;
    public string WakePhrase { get; set; } = "ねぇ、るか";
    public bool TtsEnabled { get; set; } = true;
    public int TtsRate { get; set; }
    public int TtsVolume { get; set; } = 100;
}
