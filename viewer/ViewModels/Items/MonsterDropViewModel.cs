using System.Globalization;
using Backpack.Viewer;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MonsterDropViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string      Name          { get; }
    public BitmapImage Quality       { get; }
    public string      CountText     { get; }

    public Visibility CountVisibility => (!string.IsNullOrEmpty(CountText)).ToVisibility();

    public MonsterDropViewModel(MonsterMetaService.MonsterDrop drop)
    {
        Name    = drop.Name;
        Quality = StaticResources.GetQualityBitmap(drop.Rank);

        CountText = drop.Count is { } c && double.TryParse(c, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v > 0
            ? $"×{v:0.##}"
            : string.Empty;

        if (StaticResources.DropIcon(drop.Icon) is { } uri)
            GfxLoader.BeginLoad(uri, this);
    }
}
