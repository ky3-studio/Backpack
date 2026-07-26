using System.ComponentModel;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class MaterialViewModel : INotifyPropertyChanged, IIconUpdatable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string       Name          { get; }
    public string       Count         { get; }
    public BitmapImage  QualitySource { get; }

    private BitmapImage? _iconSource;
    public  BitmapImage? IconSource
    {
        get => _iconSource;
        set
        {
            if (_iconSource == value) return;
            _iconSource = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconSource)));
        }
    }

    public MaterialViewModel(MaterialEntry entry, MaterialMetaService meta)
    {
        Name  = entry.Name;
        Count = entry.Count.ToString("N0");

        var (iconUri, rank) = meta.GetMeta(entry.Id);
        if (iconUri is not null)
            _iconSource = new BitmapImage(iconUri);

        QualitySource = new BitmapImage(StaticResources.QualityIcon(rank));
    }
}
