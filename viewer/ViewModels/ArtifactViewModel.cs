using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class ArtifactViewModel
{
    public ArtifactEntry Source             { get; }
    public string                    RankDisplay         { get; }
    public string                    Level               { get; }
    public string                    SlotRankEquipped    { get; }
    public string                    MainStatValueDisplay { get; }
    public IReadOnlyList<SubStatItemViewModel> SubStatItems       { get; }
    public Visibility                HasInstanceVisibility { get; }
    public BitmapImage?  IconSource          { get; }
    public BitmapImage   QualitySource       { get; }

    public ArtifactViewModel(ArtifactEntry entry, ArtifactMetaService meta)
    {
        Source      = entry;
        RankDisplay = new string('\u2605', Math.Clamp(entry.Rank, 0, 5));

        var hasInstance = !string.IsNullOrEmpty(entry.Guid);
        Level        = hasInstance ? $"+{entry.Level}" : string.Empty;
        SubStatItems = hasInstance
            ? entry.SubStats.Select(s => new SubStatItemViewModel(
                s.Type,
                s.Value == Math.Floor(s.Value) ? ((long)s.Value).ToString() : s.Value.ToString("F1"),
                RollsToCircle(s.Rolls)))
              .ToList()
            : System.Array.Empty<SubStatItemViewModel>();
        HasInstanceVisibility = hasInstance ? Visibility.Visible : Visibility.Collapsed;

        if (hasInstance && !string.IsNullOrEmpty(entry.MainStat.TypeRaw))
        {
            var v = meta.GetMainPropValue(entry.Rank, entry.Level, entry.MainStat.TypeRaw);
            MainStatValueDisplay = IsMainPropPercent(entry.MainStat.TypeRaw)
                ? $"{v * 100f:F1}%"
                : ((int)Math.Round(v)).ToString();
        }
        else
        {
            MainStatValueDisplay = string.Empty;
        }

        var slotParts = new System.Collections.Generic.List<string> { entry.Slot, RankDisplay };
        if (hasInstance && entry.Equipped) slotParts.Add(Localized.Get("Equipped"));
        SlotRankEquipped = string.Join("  ", slotParts);

        var iconUri = meta.GetIcon(entry.SetName, entry.Slot);
        if (iconUri is not null)
            IconSource = new BitmapImage(iconUri);

        QualitySource = new BitmapImage(
            new Uri($"ms-appx:///Assets/Quality/{RankToQualityName(entry.Rank)}.png"));
    }

    private static bool IsMainPropPercent(string propTypeRaw) => propTypeRaw is not
        ("FIGHT_PROP_HP" or "FIGHT_PROP_ATTACK" or "FIGHT_PROP_DEFENSE" or "FIGHT_PROP_ELEMENT_MASTERY");

    private static string RollsToCircle(int rolls) => rolls switch
    {
        0 => "\u24ea",  // ⓪
        1 => "\u2460",  // ①
        2 => "\u2461",  // ②
        3 => "\u2462",  // ③
        4 => "\u2463",  // ④
        _ => rolls.ToString()
    };

    private static string RankToQualityName(int rank) => rank switch
    {
        5 => "UI_QUALITY_ORANGE",
        4 => "UI_QUALITY_PURPLE",
        3 => "UI_QUALITY_BLUE",
        2 => "UI_QUALITY_GREEN",
        _ => "UI_QUALITY_WHITE",
    };
}
