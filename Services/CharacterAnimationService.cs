using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RukaDesktopAssistant.Services;

public sealed class CharacterAnimationService
{
    private readonly CharacterAssetService _assets;
    private readonly Image _image;

    public CharacterAnimationService(CharacterAssetService assets, Image image)
    {
        _assets = assets;
        _image = image;
    }

    public void SetState(string state)
    {
        var filename = state switch
        {
            "talk" => "ruka-talk.png",
            "sleep" => "ruka-sleep.png",
            "walk" => "ruka-walk-1.png",
            _ => "ruka-idle.png"
        };

        BitmapImage? image = _assets.TryLoad(filename);
        if (image is null && filename != "ruka-idle.png")
            image = _assets.TryLoad("ruka-idle.png");

        if (image is not null)
        {
            _image.Source = image;
            _image.Visibility = Visibility.Visible;
        }
    }
}
