using Backpack.Viewer;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class IngredientViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string     Name             { get; }
    public string     HeldText         { get; }
    public Visibility EnoughVisibility { get; }
    public Visibility ShortVisibility  { get; }

    public IngredientViewModel(FoodMetaService.IngredientMeta meta, ulong held, Uri? iconUri)
    {
        Name             = meta.Name;
        HeldText         = $"{held}/{meta.Amount}";
        var enough       = held >= (ulong)meta.Amount;
        EnoughVisibility = enough.ToVisibility();
        ShortVisibility  = (!enough).ToVisibility();

        if (iconUri is not null)
            GfxLoader.BeginLoad(iconUri, this);
    }
}
