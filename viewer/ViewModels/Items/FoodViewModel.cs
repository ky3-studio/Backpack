using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace Backpack.Viewer.ViewModels;

public sealed partial class FoodViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string         Name                { get; }
    public string         Count               { get; }
    public string         Character           { get; }
    public string         IngredientsText     { get; }
    public string         VariantLabel        { get; }
    public SolidColorBrush VariantForeground  { get; }
    public BitmapImage    QualitySource       { get; }
    public Visibility     CharacterVisibility { get; }
    public IReadOnlyList<IngredientViewModel> Ingredients { get; }

    public FoodViewModel(FoodMetaService.FoodMeta meta, ulong count, IReadOnlyList<IngredientViewModel> ingredients)
    {
        Name                = meta.Name;
        Count               = count.ToString("N0");
        Character           = meta.Character;
        CharacterVisibility = (!string.IsNullOrEmpty(meta.Character)).ToVisibility();

        VariantLabel = meta.Variant switch
        {
            FoodVariants.Suspicious => Localized.Get("FoodVariantSuspicious"),
            FoodVariants.Delicious  => Localized.Get("FoodVariantDelicious"),
            FoodVariants.Special    => Localized.Get("FoodVariantSpecial"),
            FoodVariants.Sweet      => Localized.Get("FoodVariantSweet"),
            _                       => Localized.Get("FoodVariantNormal"),
        };

        VariantForeground = new SolidColorBrush(meta.Variant switch
        {
            FoodVariants.Suspicious => Color.FromArgb(255, 157, 105, 213),
            FoodVariants.Delicious  => Color.FromArgb(255, 193, 148,  48),
            FoodVariants.Special    => Color.FromArgb(255,  56, 165,  90),
            FoodVariants.Sweet      => Color.FromArgb(255, 214,  89, 151),
            _                       => Color.FromArgb(160, 140, 140, 140),
        });

        GfxLoader.BeginLoad(StaticResources.MaterialIcon(meta.Icon), this);
        QualitySource   = StaticResources.GetQualityBitmap(meta.Rank);
        Ingredients     = ingredients;
        IngredientsText = string.Join("  ", meta.Ingredients.Select(i => $"{i.Name} ×{i.Amount}"));
    }
}
