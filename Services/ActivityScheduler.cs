using System.Windows.Threading;
using RukaDesktopAssistant.Models;

namespace RukaDesktopAssistant.Services;

public sealed class ActivityScheduler
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(30) };
    private readonly RukaState _state;
    private readonly Action<string> _say;
    private readonly Random _random = new();
    private DateTime _lastActivityChange = DateTime.Now;

    public ActivityScheduler(RukaState state, Action<string> say)
    {
        _state = state;
        _say = say;
        _timer.Tick += (_, _) => Tick();
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void Tick()
    {
        if (_state.IsPaused) return;
        var now = DateTime.Now;
        var minutes = (now - _lastActivityChange).TotalMinutes;
        if (minutes < 3) return;
        _lastActivityChange = now;

        // Small, non-intrusive autonomous behavior. AI/voice will be layered on later.
        if (now.Hour >= 0 && now.Hour < 7)
        {
            _state.IsSleeping = true;
            _state.Activity = "sleep";
            return;
        }

        _state.IsSleeping = false;
        var activities = new[] { "idle", "look_around", "stretch", "walk" };
        _state.Activity = activities[_random.Next(activities.Length)];
    }
}
