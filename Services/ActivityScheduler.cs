using System.Windows.Threading;
using RukaDesktopAssistant.Models;

namespace RukaDesktopAssistant.Services;

public sealed class ActivityScheduler
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(15) };
    private readonly RukaState _state;
    private readonly Action<string> _say;
    private readonly Random _random = new();
    private readonly GameProfileStore _gameProfiles = new();
    private DateTime _lastActivityChange = DateTime.Now;

    public bool IsGameSuppressed { get; private set; }
    public string? CurrentProcessName { get; private set; }

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

        var context = AppContextService.GetForegroundContext();
        CurrentProcessName = context.ProcessName;

        var profile = context.ProcessName is null
            ? new GameProfile(true, 1, "side", false)
            : _gameProfiles.Get(context.ProcessName);

        IsGameSuppressed = context.IsKnownGame && !profile.ShowCharacter;

        var now = DateTime.Now;
        if (now.Hour >= 0 && now.Hour < 7)
        {
            _state.IsSleeping = true;
            _state.Activity = "sleep";
            return;
        }

        _state.IsSleeping = false;
        if ((now - _lastActivityChange).TotalMinutes < 1) return;
        _lastActivityChange = now;

        var activities = IsGameSuppressed
            ? new[] { "watching_game", "idle" }
            : new[] { "idle", "look_around", "stretch", "walk" };

        _state.Activity = activities[_random.Next(activities.Length)];
    }
}
