using Backpack.Viewer;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class OwnedArtifactViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public ArtifactEntry              Source          { get; }
    public string                     SlotName        { get; }
    public string                     PieceName       { get; }
    public string                     LevelText       { get; }
    public string                     MainStatName    { get; }
    public string                     MainStatValue   { get; }
    public int                        Rank            { get; }
    public BitmapImage                RankStarsSource { get; }
    public BitmapImage                QualitySource   { get; }
    public IReadOnlyList<OwnedSubStat> SubStats       { get; }

    public OwnedArtifactViewModel(ArtifactEntry entry, ArtifactMetaService meta)
    {
        Source          = entry;
        SlotName        = entry.Slot;
        PieceName       = entry.Name;
        LevelText       = $"+{entry.Level}";
        Rank            = entry.Rank;
        RankStarsSource = StaticResources.GetRankStarsBitmap(entry.Rank);
        QualitySource   = StaticResources.GetQualityBitmap(entry.Rank);

        MainStatName  = entry.MainStat;
        var mainValue = meta.GetMainPropValue(entry.Rank, entry.Level, entry.MainStat);
        MainStatValue = FormatValue(entry.MainStat, mainValue);

        SubStats = [.. entry.SubStats.Select(s =>
        {
            var count = s.Rolls?.Length ?? 0;
            return new OwnedSubStat(
                s.Type,
                FormatSub(s.Type, s.Value),
                StaticResources.RollBadge(count),
                FormatRollValues(s.Type, s.Rolls),
                count > 0 ? Visibility.Visible : Visibility.Collapsed);
        })];

        var icon = meta.GetIcon(entry.Set, entry.Slot);
        if (icon is not null)
            GfxLoader.BeginLoad(icon, this);
    }

    private static string FormatValue(string shortName, float value) =>
        ArtifactMetaService.IsMainPropPercent(shortName)
            ? $"{value * 100f:F1}%"
            : ((int)Math.Round(value)).ToString();

    private static string FormatSub(string shortName, double value) =>
        ArtifactMetaService.IsMainPropPercent(shortName)
            ? $"{value:F1}%"
            : ((int)Math.Round(value)).ToString();

    private static string FormatRollValues(string shortName, double[]? rolls)
    {
        if (rolls is null || rolls.Length == 0) return string.Empty;
        var isPct = ArtifactMetaService.IsMainPropPercent(shortName);
        var parts = rolls.Select(v => isPct ? v.ToString("F1") : ((int)Math.Round(v)).ToString());
        return string.Join(" + ", parts);
    }
}

public sealed record OwnedSubStat(string Name, string Value, BitmapImage? RollBadge, string RollValues, Visibility RollVisibility);
