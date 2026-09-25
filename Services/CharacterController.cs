using System.Windows;
using System.Windows.Media.Animation;

namespace RukaDesktopAssistant.Services;

public sealed class CharacterController
{
    private readonly Window _window;
    private readonly Random _random = new();

    public bool IsMoving { get; private set; }

    public CharacterController(Window window) => _window = window;

    public void StopMovement()
    {
        _window.BeginAnimation(Window.LeftProperty, null);
        _window.BeginAnimation(Window.TopProperty, null);
        IsMoving = false;
    }

    public void MoveTo(double left, double top, TimeSpan? duration = null)
    {
        var d = duration ?? TimeSpan.FromMilliseconds(900);
        IsMoving = true;

        var leftAnim = new DoubleAnimation(_window.Left, left, d)
        {
            FillBehavior = FillBehavior.Stop
        };
        var topAnim = new DoubleAnimation(_window.Top, top, d)
        {
            FillBehavior = FillBehavior.Stop
        };

        leftAnim.Completed += (_, _) =>
        {
            _window.Left = left;
            _window.Top = top;
            IsMoving = false;
        };

        _window.BeginAnimation(Window.LeftProperty, leftAnim);
        _window.BeginAnimation(Window.TopProperty, topAnim);
    }

    public void MoveToMonitor(int monitorIndex)
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (monitorIndex < 0 || monitorIndex >= screens.Length) return;

        var work = screens[monitorIndex].WorkingArea;
        var targetLeft = work.Left + Math.Max(20, work.Width - (int)_window.Width - 40);
        var targetTop = work.Top + Math.Max(20, work.Height - (int)_window.Height - 40);
        MoveTo(targetLeft, targetTop, TimeSpan.FromSeconds(1.2));
    }

    public void Wander()
    {
        if (IsMoving) return;
        var point = new System.Drawing.Point((int)_window.Left, (int)_window.Top);
        var screen = System.Windows.Forms.Screen.FromPoint(point);
        var work = screen.WorkingArea;

        var maxX = Math.Max(work.Left + 20, work.Right - (int)_window.Width - 20);
        var maxY = Math.Max(work.Top + 20, work.Bottom - (int)_window.Height - 20);
        var targetLeft = _random.Next(work.Left + 20, maxX + 1);
        var targetTop = _random.Next(work.Top + 20, maxY + 1);

        MoveTo(targetLeft, targetTop);
    }
}
