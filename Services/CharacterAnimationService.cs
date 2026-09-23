using System.Windows.Media.Imaging;
using WpfImage = System.Windows.Controls.Image;

namespace RukaDesktopAssistant.Services;

public sealed class CharacterAnimationService
{
    private readonly CharacterAssetService _assets;
    private readonly WpfImage _image;

    public CharacterAnimationService(CharacterAssetService assets, WpfImage image)
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
            _image.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
