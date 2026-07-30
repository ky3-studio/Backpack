using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class WeaponRecommendAvatar
{
    public WeaponRecommendAvatar(BitmapImage icon, BitmapImage quality, string name)
    {
        Icon = icon;
        Quality = quality;
        Name = name;
    }

    public BitmapImage Icon    { get; }
    public BitmapImage Quality { get; }
    public string      Name    { get; }
}
