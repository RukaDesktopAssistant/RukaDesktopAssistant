using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RukaDesktopAssistant.Services;

public sealed class CharacterAnimationService
{
    private readonly System.Windows.Shapes.Path _normalMouth;
    private readonly FrameworkElement _talkMouth;
    private readonly FrameworkElement _sleepFace;
    private readonly FrameworkElement _walkFace;

    public CharacterAnimationService(
        System.Windows.Shapes.Path normalMouth,
        FrameworkElement talkMouth,
        FrameworkElement sleepFace,
        FrameworkElement walkFace)
    {
        _normalMouth = normalMouth;
        _talkMouth = talkMouth;
        _sleepFace = sleepFace;
        _walkFace = walkFace;
    }

    public void SetState(string state)
    {
        _normalMouth.Visibility = Visibility.Visible;
        _talkMouth.Visibility = Visibility.Collapsed;
        _sleepFace.Visibility = Visibility.Collapsed;
        _walkFace.Visibility = Visibility.Collapsed;

        switch (state)
        {
            case "talk":
                _normalMouth.Visibility = Visibility.Collapsed;
                _talkMouth.Visibility = Visibility.Visible;
                break;

            case "sleep":
                _normalMouth.Visibility = Visibility.Collapsed;
                _sleepFace.Visibility = Visibility.Visible;
                break;

            case "walk":
                _walkFace.Visibility = Visibility.Visible;
                break;
        }
    }
}
