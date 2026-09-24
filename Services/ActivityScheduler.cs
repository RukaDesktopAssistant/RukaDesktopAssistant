using System.Windows.Threading;
using RukaDesktopAssistant.Models;

namespace RukaDesktopAssistant.Services;

public sealed class ActivityScheduler
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(15) };
    private readonly RukaState _state;
    private readonly Action<string> _say;
    private readonly CharacterController _character;
    private readonly Random _random = new();
    private readonly GameProfileStore _gameProfiles = new();
    private DateTime _lastActivityChange = DateTime.MinValue;

    public bool IsGameSuppressed { get; private set; }
    public string? CurrentProcessName { get; private set; }
    public GameProfile CurrentGameProfile { get; private set; } = new(true, 1, "side", false, true);
    public event Action<AppContextInfo, GameProfile>? ContextChanged;
    public event Action<string>? ActivityChanged;

    public ActivityScheduler(RukaState state, Action<string> say, CharacterController character)
    {
        _state = state;
        _say = say;
        _character = character;
        _timer.Tick += (_, _) => Tick();
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void Tick()
    {
        if (_state.IsPaused) return;
        var context = AppContextService.GetForegroundContext();
        CurrentProcessName = context.ProcessName;
        var profile = context.ProcessName is null ? new GameProfile(true, 1, "side", false, true) : _gameProfiles.Get(context.ProcessName);
        CurrentGameProfile = profile;
        IsGameSuppressed = context.IsKnownGame && !profile.ShowCharacter;
        ContextChanged?.Invoke(context, profile);

        var now = DateTime.Now;
        if (now.Hour >= 0 && now.Hour < 7)
        {
            SetActivity("sleep");
            return;
        }

        _state.IsSleeping = false;
        if ((now - _lastActivityChange).TotalMinutes < 1) return;
        _lastActivityChange = now;

        var activities = context.IsKnownGame
            ? new[] { "watching_game", "idle", "look_around" }
            : new[] { "idle", "look_around", "stretch", "walk" };
        SetActivity(activities[_random.Next(activities.Length)]);
    }

    private void SetActivity(string activity)
    {
        if (string.Equals(_state.Activity, activity, StringComparison.OrdinalIgnoreCase)) return;
        _state.Activity = activity;
        _state.IsSleeping = activity == "sleep";
        ActivityChanged?.Invoke(activity);
    }
}