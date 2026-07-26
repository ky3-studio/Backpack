using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class WeaponViewModel
{
    public WeaponEntry  Source             { get; }
    public string       RankDisplay         { get; }
    public string       Level               { get; }
    public string       Refine              { get; }
    public string       TypeRankDisplay     { get; }
    public Visibility   HasInstanceVisibility { get; }
    public BitmapImage? IconSource          { get; }
    public BitmapImage  QualitySource       { get; }

    public WeaponViewModel(WeaponEntry entry, WeaponMetaService meta)
    {
        Source      = entry;
        RankDisplay = new string('\u2605', Math.Clamp(entry.Rank, 0, 5));

        var hasInstance = !string.IsNullOrEmpty(entry.Guid);
        Level         = hasInstance ? $"Lv.{entry.Level}" : string.Empty;
        Refine        = hasInstance ? $"R{entry.Refine}"  : string.Empty;
        HasInstanceVisibility = hasInstance ? Visibility.Visible : Visibility.Collapsed;
        TypeRankDisplay = $"{entry.Type}  {RankDisplay}";

        var iconUri = meta.GetIcon(entry.Id);
        if (iconUri is not null)
            IconSource = new BitmapImage(iconUri);

        QualitySource = new BitmapImage(
            new Uri($"ms-appx:///Assets/Quality/{RankToQualityName(entry.Rank)}.png"));
    }

    private static string RankToQualityName(int rank) => rank switch
    {
        5 => "UI_QUALITY_ORANGE",
        4 => "UI_QUALITY_PURPLE",
        3 => "UI_QUALITY_BLUE",
        2 => "UI_QUALITY_GREEN",
        _ => "UI_QUALITY_WHITE",
    };
}
