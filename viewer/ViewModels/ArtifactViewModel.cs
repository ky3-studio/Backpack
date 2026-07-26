using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class ArtifactViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public ArtifactEntry                       Source                { get; }
    public string                              RankDisplay           { get; }
    public string                              Level                 { get; }
    public string                              SlotRankEquipped      { get; }
    public string                              MainStatValueDisplay  { get; }
    public string                              BonusText             { get; }
    public IReadOnlyList<SubStatItemViewModel> SubStatItems          { get; }
    public Visibility                          HasInstanceVisibility { get; }
    public Visibility                          HasAnyBonusVisibility { get; }
    public BitmapImage                         QualitySource         { get; }

    public ArtifactViewModel(ArtifactEntry entry, ArtifactMetaService meta)
    {
        Source      = entry;
        RankDisplay = new string('\u2605', Math.Clamp(entry.Rank, 0, 5));

        var hasInstance = !string.IsNullOrEmpty(entry.Guid);
        Level        = hasInstance ? $"+{entry.Level}" : string.Empty;
        SubStatItems = hasInstance
            ? entry.SubStats.Select(s =>
            {
                static string Fmt(double v) =>
                    v == Math.Floor(v) ? ((long)v).ToString() : v.ToString("F1");
                string valueDisplay = s.Rolls.Length > 1
                    ? $"{string.Join(" + ", s.Rolls.Select(Fmt))} = {Fmt(s.Value)}"
                    : Fmt(s.Value);
                return new SubStatItemViewModel(
                    s.Type,
                    valueDisplay,
                    new BitmapImage(new Uri($"ms-appx:///Assets/badge/badge-{Math.Clamp(s.Rolls.Length, 1, 11)}.ico")));
            }).ToList()
            : System.Array.Empty<SubStatItemViewModel>();
        HasInstanceVisibility = hasInstance.ToVisibility();

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
        if (hasInstance && entry.Locked) slotParts.Add(Localized.Get("Locked"));
        SlotRankEquipped = string.Join("  ", slotParts);

        var iconUri = meta.GetIcon(entry.SetName, entry.Slot);
        if (iconUri is not null)
            _iconSource = new BitmapImage(iconUri);

        var allBonuses = meta.GetAllSetBonuses(entry.SetName);
        BonusText             = string.Join("\n", allBonuses.Select(b => $"{b.Count}件套：{b.Desc}"));
        HasAnyBonusVisibility = (allBonuses.Count > 0).ToVisibility();

        QualitySource = new BitmapImage(StaticResources.QualityIcon(entry.Rank));
    }

    private static bool IsMainPropPercent(string propTypeRaw) => propTypeRaw is not
        ("FIGHT_PROP_HP" or "FIGHT_PROP_ATTACK" or "FIGHT_PROP_DEFENSE" or "FIGHT_PROP_ELEMENT_MASTERY");
}
