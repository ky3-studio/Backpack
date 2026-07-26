using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class MaterialViewModel
{
    public string       Name          { get; }
    public string       Count         { get; }
    public BitmapImage? IconSource    { get; }
    public BitmapImage  QualitySource { get; }

    public MaterialViewModel(MaterialEntry entry, MaterialMetaService meta)
    {
        Name  = entry.Name;
        Count = entry.Count.ToString("N0");

        var (iconUri, rank) = meta.GetMeta(entry.Id);
        if (iconUri is not null)
            IconSource = new BitmapImage(iconUri);

        QualitySource = new BitmapImage(
            new Uri($"ms-appx:///Assets/Quality/{RankToQualityName(rank)}.png"));
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
