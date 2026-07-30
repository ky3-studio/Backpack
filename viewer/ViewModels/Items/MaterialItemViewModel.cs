using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MaterialItemViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string      Name    { get; }
    public BitmapImage Quality { get; }

    public MaterialItemViewModel(uint id, MaterialMetaService meta)
    {
        Name = meta.GetName(id);
        var (iconUri, rank) = meta.GetMeta(id);
        Quality = StaticResources.GetQualityBitmap(rank);
        if (iconUri is not null)
            GfxLoader.BeginLoad(iconUri, this);
    }
}
