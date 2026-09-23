using System.Windows;
using System.Windows.Media.Animation;

namespace RukaDesktopAssistant.Services;

public sealed class CharacterController
{
    private readonly Window _window;
    private readonly Random _random = new();

    public CharacterController(Window window) => _window = window;

    public void MoveTo(double left, double top, TimeSpan? duration = null)
    {
        var d = duration ?? TimeSpan.FromMilliseconds(700);
        var leftAnim = new DoubleAnimation(_window.Left, left, d);
        var topAnim = new DoubleAnimation(_window.Top, top, d);
        _window.BeginAnimation(Window.LeftProperty, leftAnim);
        _window.BeginAnimation(Window.TopProperty, topAnim);
    }

    public void Wander()
    {
        var work = System.Windows.Forms.Screen.FromPoint(
            new System.Drawing.Point((int)_window.Left, (int)_window.Top)).WorkingArea;

        var targetLeft = work.Left + _random.Next(20, Math.Max(21, work.Width - 220));
        var targetTop = work.Top + _random.Next(20, Math.Max(21, work.Height - 220));
        MoveTo(targetLeft, targetTop);
    }
}
