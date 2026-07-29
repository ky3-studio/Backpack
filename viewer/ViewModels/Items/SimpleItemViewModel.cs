using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public abstract partial class SimpleItemViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string      Name          { get; }
    public string      Count         { get; }
    public BitmapImage QualitySource { get; }

    protected SimpleItemViewModel(string name, ulong count, (Uri? IconUri, int Rank) meta)
    {
        Name  = name;
        Count = count.ToString("N0");
        if (meta.IconUri is not null)
            GfxLoader.BeginLoad(meta.IconUri, this);
        QualitySource = StaticResources.GetQualityBitmap(meta.Rank);
    }
}
