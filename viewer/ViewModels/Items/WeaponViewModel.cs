using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class WeaponViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    private static readonly int[] MaxLevelByPromote = [20, 40, 50, 60, 70, 80, 90];

    public WeaponEntry  Source                { get; }
    public IReadOnlyList<int> RankItems        { get; }
    public string       Level                 { get; }
    public string       LevelFull             { get; }
    public string       RefineLabel           { get; }
    public string       Refine                { get; }
    public string       AtkDisplay            { get; }
    public string       SubDisplay            { get; }
    public string       PassiveName           { get; }
    public string       SkillDesc             { get; }
    public string       FlavorText            { get; }
    public Visibility   HasInstanceVisibility { get; }
    public Visibility   SubVisibility         { get; }
    public Visibility   PassiveVisibility     { get; }
    public Visibility   FlavorVisibility      { get; }
    public BitmapImage  QualitySource         { get; }

    public WeaponViewModel(WeaponEntry entry, WeaponMetaService meta)
    {
        Source      = entry;
        RankItems   = [.. Enumerable.Range(0, Math.Clamp(entry.Rank, 0, 5))];

        var hasInstance = !string.IsNullOrEmpty(entry.Guid);
        HasInstanceVisibility = hasInstance.ToVisibility();

        if (hasInstance)
        {
            Level       = $"{Localized.Get("LevelPrefix")}{entry.Level}";
            LevelFull   = $"{Localized.Get("LevelPrefix")}{entry.Level}/{MaxLevelByPromote[Math.Clamp(entry.Ascension, 0, 6)]}";
            RefineLabel = string.Format(Localized.Get("WeaponRefineFmt"), entry.Refine);
            Refine      = $"R{entry.Refine}";
            var (atk, sub)     = meta.CalcStats(entry.Id, entry.Level, entry.Ascension);
            var (pName, pDesc) = meta.GetSkill(entry.Id, entry.Refine);
            AtkDisplay         = atk > 0 ? atk.ToString() : string.Empty;
            SubDisplay         = sub;
            PassiveName        = pName;
            SkillDesc          = pDesc;
            FlavorText         = meta.GetFlavorText(entry.Id);
        }
        else
        {
            Level = Refine = LevelFull = RefineLabel = AtkDisplay = SubDisplay = string.Empty;
            PassiveName = meta.GetSkill(entry.Id, 1).Name;
            FlavorText  = meta.GetFlavorText(entry.Id);
            SkillDesc   = string.Empty;
        }

        SubVisibility     = (hasInstance && !string.IsNullOrEmpty(SubDisplay)).ToVisibility();
        PassiveVisibility = (!string.IsNullOrEmpty(PassiveName)).ToVisibility();
        FlavorVisibility  = (!string.IsNullOrEmpty(FlavorText)).ToVisibility();

        var iconUri = meta.GetIcon(entry.Id);
        if (iconUri is not null)
            GfxLoader.BeginLoad(iconUri, this);

        QualitySource = StaticResources.GetQualityBitmap(entry.Rank);
    }
}
