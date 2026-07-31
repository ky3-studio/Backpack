using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class ArtifactSetViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPiece))]
    [NotifyPropertyChangedFor(nameof(MainStatRows))]
    private int _selectedPieceIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    [NotifyPropertyChangedFor(nameof(MaxLevel))]
    [NotifyPropertyChangedFor(nameof(RankStarsSource))]
    [NotifyPropertyChangedFor(nameof(MainStatRows))]
    private int _selectedRankIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LevelText))]
    [NotifyPropertyChangedFor(nameof(MainStatRows))]
    private double _level;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisibleOwnedPieces))]
    private int _selectedOwnedRankIndex;

    private readonly ArtifactMetaService _meta;

    public string                                SetName              { get; }
    public Uri?                                  IconUri              { get; }
    public string                                BonusText            { get; }
    public Visibility                            BonusVisibility      { get; }
    public IReadOnlyList<ArtifactPieceViewModel> Pieces               { get; }
    public IReadOnlyList<int>                    Ranks                { get; }
    public IReadOnlyList<ArtifactRankOption>     RankOptions          { get; }
    public Visibility                            RankSwitchVisibility { get; }

    public IReadOnlyList<OwnedArtifactViewModel> OwnedPieces               { get; private set; } = [];
    public IReadOnlyList<ArtifactRankOption>     OwnedRankOptions          { get; private set; } = [];
    public Visibility                            OwnedVisibility           { get; private set; } = Visibility.Collapsed;
    public Visibility                            OwnedRankSwitchVisibility { get; private set; } = Visibility.Collapsed;
    public string                                OwnedCountText            { get; private set; } = string.Empty;

    public IReadOnlyList<OwnedArtifactViewModel> VisibleOwnedPieces =>
        OwnedRankOptions.Count == 0
            ? []
            : [.. OwnedPieces.Where(p => p.Rank == OwnedRankOptions[Math.Clamp(SelectedOwnedRankIndex, 0, OwnedRankOptions.Count - 1)].Rank)];

    public int         Rank            => Ranks[Math.Clamp(SelectedRankIndex, 0, Ranks.Count - 1)];
    public int         MaxLevel        => Rank * 4;
    public BitmapImage RankStarsSource => StaticResources.GetRankStarsBitmap(Rank);

    public ArtifactPieceViewModel? SelectedPiece =>
        SelectedPieceIndex >= 0 && SelectedPieceIndex < Pieces.Count ? Pieces[SelectedPieceIndex] : null;

    public string LevelText => $"Lv.{(int)Level}";

    public IReadOnlyList<ArtifactStatRow> MainStatRows =>
        SelectedPiece is { } p
            ? [.. p.MainStatKeys.Select(k => new ArtifactStatRow(k, FormatValue(k, _meta.GetMainPropValue(Rank, (int)Level, k))))]
            : [];

    public ArtifactSetViewModel(string setName, ArtifactMetaService meta)
    {
        _meta   = meta;
        SetName = setName;

        var ranks = meta.GetSetRanks(setName);
        Ranks       = ranks.Count > 0 ? ranks : [5];
        RankOptions = [.. Ranks.Select(r => new ArtifactRankOption(r, StaticResources.GetRankStarsBitmap(r)))];
        RankSwitchVisibility = (Ranks.Count > 1).ToVisibility();

        _selectedRankIndex = Ranks.Count - 1;
        _level             = MaxLevel;

        IconUri = meta.GetSetIcon(setName);

        var bonuses = meta.GetAllSetBonuses(setName);
        BonusText       = string.Join("\n", bonuses.Select(b => string.Format(SR.SetBonusFmt, b.Count, b.Desc)));
        BonusVisibility = (bonuses.Count > 0).ToVisibility();

        Pieces = [.. meta.GetSetSlots(setName).Select(s => new ArtifactPieceViewModel(setName, s, meta))];

        if (IconUri is not null)
            GfxLoader.BeginLoad(IconUri, this);
    }

    public void AttachOwned(IReadOnlyList<ArtifactEntry> owned)
    {
        OwnedPieces = [.. owned.Select(e => new OwnedArtifactViewModel(e, _meta))];
        var ranks = OwnedPieces.Select(p => p.Rank).Distinct().OrderByDescending(r => r).ToArray();
        OwnedRankOptions          = [.. ranks.Select(r => new ArtifactRankOption(r, StaticResources.GetRankStarsBitmap(r)))];
        OwnedVisibility           = (OwnedPieces.Count > 0).ToVisibility();
        OwnedRankSwitchVisibility = (ranks.Length > 1).ToVisibility();
        OwnedCountText            = string.Format(SR.OwnedCountFmt, OwnedPieces.Count);
    }

    partial void OnSelectedRankIndexChanged(int value)
    {
        if (Level > MaxLevel)
            Level = MaxLevel;
    }

    private static string FormatValue(string shortName, float value) =>
        ArtifactMetaService.IsMainPropPercent(shortName)
            ? $"{value * 100f:F1}%"
            : ((int)Math.Round(value)).ToString();
}

public sealed record ArtifactStatRow(string Name, string Value);

public sealed record ArtifactRankOption(int Rank, BitmapImage Stars);
