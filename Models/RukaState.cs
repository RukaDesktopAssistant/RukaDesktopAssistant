namespace RukaDesktopAssistant.Models;

public sealed class RukaState
{
    public bool IsPaused { get; set; }
    public bool IsSleeping { get; set; }
    public string Activity { get; set; } = "idle";
    public DateTime SessionStartedAt { get; set; } = DateTime.Now;
}
