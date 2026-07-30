using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class WeaponRecommendAvatar : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public BitmapImage Quality { get; }
    public string      Name    { get; }

    public WeaponRecommendAvatar(Uri iconUri, BitmapImage quality, string name)
    {
        Quality = quality;
        Name = name;
        GfxLoader.BeginLoad(iconUri, this);
    }
}
