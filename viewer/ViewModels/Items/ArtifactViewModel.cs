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

    private static readonly BitmapImage[] _badges =
    [
        .. Enumerable.Range(1, 11).Select(i => new BitmapImage(new Uri($"ms-appx:///Assets/badge/badge-{i}.ico")))
    ];

    public ArtifactEntry                       Source                { get; }
    public IReadOnlyList<int>                  RankItems             { get; }
    public string                              Level                 { get; }
    public string                              SlotText              { get; }
    public string                              MainStatValueDisplay  { get; }
    public string                              BonusText             { get; }
    public IReadOnlyList<SubStatItemViewModel> SubStatItems          { get; }
    public Visibility                          HasInstanceVisibility { get; }
    public Visibility                          HasAnyBonusVisibility { get; }
    public BitmapImage                         QualitySource         { get; }
    public Uri?                                IconUri               { get; }

    public SubStatItemViewModel? SubStat0 => SubStatItems.Count > 0 ? SubStatItems[0] : null;
    public SubStatItemViewModel? SubStat1 => SubStatItems.Count > 1 ? SubStatItems[1] : null;
    public SubStatItemViewModel? SubStat2 => SubStatItems.Count > 2 ? SubStatItems[2] : null;
    public SubStatItemViewModel? SubStat3 => SubStatItems.Count > 3 ? SubStatItems[3] : null;
    public Visibility SubStat0Vis => (SubStatItems.Count > 0).ToVisibility();
    public Visibility SubStat1Vis => (SubStatItems.Count > 1).ToVisibility();
    public Visibility SubStat2Vis => (SubStatItems.Count > 2).ToVisibility();
    public Visibility SubStat3Vis => (SubStatItems.Count > 3).ToVisibility();

    // flat 属性：避免链式 Binding 的两次反射开销
    public string        SubStat0Name  => SubStatItems.Count > 0 ? SubStatItems[0].Name : string.Empty;
    public string        SubStat0Value => SubStatItems.Count > 0 ? SubStatItems[0].ValueDisplay : string.Empty;
    public BitmapImage?  SubStat0Badge => SubStatItems.Count > 0 ? SubStatItems[0].BadgeSource : null;
    public string        SubStat1Name  => SubStatItems.Count > 1 ? SubStatItems[1].Name : string.Empty;
    public string        SubStat1Value => SubStatItems.Count > 1 ? SubStatItems[1].ValueDisplay : string.Empty;
    public BitmapImage?  SubStat1Badge => SubStatItems.Count > 1 ? SubStatItems[1].BadgeSource : null;
    public string        SubStat2Name  => SubStatItems.Count > 2 ? SubStatItems[2].Name : string.Empty;
    public string        SubStat2Value => SubStatItems.Count > 2 ? SubStatItems[2].ValueDisplay : string.Empty;
    public BitmapImage?  SubStat2Badge => SubStatItems.Count > 2 ? SubStatItems[2].BadgeSource : null;
    public string        SubStat3Name  => SubStatItems.Count > 3 ? SubStatItems[3].Name : string.Empty;
    public string        SubStat3Value => SubStatItems.Count > 3 ? SubStatItems[3].ValueDisplay : string.Empty;
    public BitmapImage?  SubStat3Badge => SubStatItems.Count > 3 ? SubStatItems[3].BadgeSource : null;

    // 套装加成简要版：每条效果截取前 32 字，避免长文字 TextWrapping 换行计算拖慢滚动
    public string BonusSummary { get; }

    public ArtifactViewModel(ArtifactEntry entry, ArtifactMetaService meta)
    {
        Source      = entry;
        RankItems   = [.. Enumerable.Range(0, Math.Clamp(entry.Rank, 0, 5))];

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
                    _badges[Math.Clamp(s.Rolls.Length, 1, 11) - 1]);
            }).ToList()
            : System.Array.Empty<SubStatItemViewModel>();
        HasInstanceVisibility = hasInstance.ToVisibility();

        if (hasInstance && !string.IsNullOrEmpty(entry.MainStat))
        {
            var v = meta.GetMainPropValue(entry.Rank, entry.Level, entry.MainStat);
            MainStatValueDisplay = IsMainPropPercent(entry.MainStat)
                ? $"{v * 100f:F1}%"
                : ((int)Math.Round(v)).ToString();
        }
        else
        {
            MainStatValueDisplay = string.Empty;
        }

        var slotParts = new System.Collections.Generic.List<string> { entry.Slot };
        if (hasInstance && entry.Locked) slotParts.Add(SR.Locked);
        SlotText = string.Join("  ", slotParts);

        var iconUri = meta.GetIcon(entry.Set, entry.Slot);
        IconUri = iconUri;
        if (iconUri is not null)
            GfxLoader.BeginLoad(iconUri, this);

        var allBonuses = meta.GetAllSetBonuses(entry.Set);
        BonusText      = string.Join("\n", allBonuses.Select(b => string.Format(SR.SetBonusFmt, b.Count, b.Desc)));
        BonusSummary   = string.Join("\n", allBonuses.Select(b =>
        {
            var desc = b.Desc.Length > 32 ? b.Desc[..32] + "…" : b.Desc;
            return string.Format(SR.SetBonusFmt, b.Count, desc);
        }));
        HasAnyBonusVisibility = (allBonuses.Count > 0).ToVisibility();

        QualitySource = StaticResources.GetQualityBitmap(entry.Rank);
    }

    private static bool IsMainPropPercent(string mainStat) => mainStat is not
        (PropShortNames.Hp or PropShortNames.Attack or PropShortNames.Defense or PropShortNames.ElementMastery);
}
