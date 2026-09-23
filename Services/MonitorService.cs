using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RukaDesktopAssistant.Services;

public sealed record MonitorInfo(string Id, Rect WorkingArea, bool Primary);

public static class MonitorService
{
    public static IReadOnlyList<MonitorInfo> GetMonitors() =>
        System.Windows.Forms.Screen.AllScreens
            .Select((s, i) => new MonitorInfo(
                s.DeviceName,
                new Rect(s.WorkingArea.Left, s.WorkingArea.Top, s.WorkingArea.Width, s.WorkingArea.Height),
                s.Primary))
            .ToList();
}
