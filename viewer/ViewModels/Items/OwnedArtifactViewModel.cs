using Backpack.Viewer;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
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
    public IReadOnlyList<OwnedSubStat> SubStats       { get; }

    public OwnedArtifactViewModel(ArtifactEntry entry, ArtifactMetaService meta)
    {
        Source          = entry;
        SlotName        = entry.Slot;
        PieceName       = entry.Name;
        LevelText       = $"+{entry.Level}";
        Rank            = entry.Rank;
        RankStarsSource = StaticResources.GetRankStarsBitmap(entry.Rank);

        MainStatName  = entry.MainStat;
        var mainValue = meta.GetMainPropValue(entry.Rank, entry.Level, entry.MainStat);
        MainStatValue = FormatValue(entry.MainStat, mainValue);

        SubStats = [.. entry.SubStats.Select(s => new OwnedSubStat(s.Type, FormatSub(s.Type, s.Value)))];

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
}

public sealed record OwnedSubStat(string Name, string Value);
