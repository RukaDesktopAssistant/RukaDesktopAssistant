namespace RukaDesktopAssistant.Models;

public sealed record MonitorProfile(
    string MonitorId,
    bool Enabled = true,
    string Position = "bottom-right",
    double Scale = 1.0);
