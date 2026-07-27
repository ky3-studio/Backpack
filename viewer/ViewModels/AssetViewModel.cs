using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class AssetViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string      Name          { get; }
    public string      Count         { get; }
    public BitmapImage QualitySource { get; }

    public AssetViewModel(MaterialEntry entry, AssetMetaService meta)
    {
        Name  = entry.Name;
        Count = entry.Count.ToString("N0");

        var (iconUri, rank) = meta.GetMeta(entry.Id);
        if (iconUri is not null)
            GfxLoader.BeginLoad(iconUri, this);

        QualitySource = new BitmapImage(StaticResources.QualityIcon(rank));
    }
}
