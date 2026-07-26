using Backpack.Viewer;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class FoodViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string      Name                { get; }
    public string      Count               { get; }
    public string      Character           { get; }
    public string      IngredientsText     { get; }
    public BitmapImage QualitySource       { get; }
    public Visibility  CharacterVisibility { get; }

    public FoodViewModel(FoodMetaService.FoodMeta meta, ulong count)
    {
        Name      = meta.Name;
        Count     = count.ToString("N0");
        Character = meta.Character;
        CharacterVisibility = (!string.IsNullOrEmpty(meta.Character)).ToVisibility();

        _iconSource   = new BitmapImage(StaticResources.MaterialIcon(meta.Icon));
        QualitySource = new BitmapImage(StaticResources.QualityIcon(meta.Rank));

        IngredientsText = string.Join("  ", meta.Ingredients.Select(i => $"{i.Name} \u00d7{i.Amount}"));
    }
}
