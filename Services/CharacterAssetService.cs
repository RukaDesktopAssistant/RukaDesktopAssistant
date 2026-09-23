using System.Windows.Media.Imaging;

namespace RukaDesktopAssistant.Services;

public sealed class CharacterAssetService
{
    public string AssetDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "Assets");

    public BitmapImage? TryLoad(string fileName)
    {
        try
        {
            var path = Path.Combine(AssetDirectory, fileName);
            if (!File.Exists(path)) return null;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }
}
