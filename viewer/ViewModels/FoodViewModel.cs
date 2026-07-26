using System.ComponentModel;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class FoodViewModel : INotifyPropertyChanged, IIconUpdatable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string       Name                { get; }
    public string       Count               { get; }
    public string       Character           { get; }
    public string       IngredientsText     { get; }
    public BitmapImage  QualitySource       { get; }
    public Visibility   CharacterVisibility { get; }

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

    public FoodViewModel(FoodMetaService.FoodMeta meta, ulong count)
    {
        Name      = meta.Name;
        Count     = count.ToString("N0");
        Character = meta.Character;
        CharacterVisibility = string.IsNullOrEmpty(meta.Character)
            ? Visibility.Collapsed
            : Visibility.Visible;

        _iconSource   = new BitmapImage(StaticResources.MaterialIcon(meta.Icon));
        QualitySource = new BitmapImage(StaticResources.QualityIcon(meta.Rank));

        IngredientsText = string.Join("  ", meta.Ingredients.Select(i => $"{i.Name} \u00d7{i.Amount}"));
    }
}
