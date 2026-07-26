using System.ComponentModel;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class WeaponViewModel : INotifyPropertyChanged, IIconUpdatable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly int[] MaxLevelByPromote = [20, 40, 50, 60, 70, 80, 90];

    public WeaponEntry  Source                { get; }
    public string       RankDisplay           { get; }
    public string       Level                 { get; }
    public string       LevelFull             { get; }
    public string       RefineLabel           { get; }
    public string       Refine                { get; }
    public string       AtkDisplay            { get; }
    public string       SubDisplay            { get; }
    public string       TypeRankDisplay       { get; }
    public string       PassiveName           { get; }
    public string       SkillDesc             { get; }
    public string       FlavorText            { get; }
    public Visibility   HasInstanceVisibility { get; }
    public Visibility   SubVisibility         { get; }
    public Visibility   PassiveVisibility     { get; }
    public Visibility   FlavorVisibility      { get; }
    public BitmapImage  QualitySource         { get; }

    private BitmapImage? _iconSource;
    public  BitmapImage? IconSource
    {
        get => _iconSource;
        set
        {
            if (_iconSource == value) return;
            _iconSource = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconSource)));
        }
    }

    public WeaponViewModel(WeaponEntry entry, WeaponMetaService meta)
    {
        Source      = entry;
        RankDisplay = new string('\u2605', Math.Clamp(entry.Rank, 0, 5));

        var hasInstance = !string.IsNullOrEmpty(entry.Guid);
        HasInstanceVisibility = hasInstance ? Visibility.Visible : Visibility.Collapsed;
        TypeRankDisplay       = $"{entry.Type}  {RankDisplay}";

        if (hasInstance)
        {
            Level       = $"Lv.{entry.Level}";
            LevelFull   = $"Lv.{entry.Level}/{MaxLevelByPromote[Math.Clamp(entry.Promote, 0, 6)]}";
            RefineLabel = $"精炼{entry.Refine}阶";
            Refine      = $"R{entry.Refine}";
            var (atk, sub)     = meta.CalcStats(entry.Id, entry.Level, entry.Promote);
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

        SubVisibility     = hasInstance && !string.IsNullOrEmpty(SubDisplay)
            ? Visibility.Visible
            : Visibility.Collapsed;
        PassiveVisibility = !string.IsNullOrEmpty(PassiveName)
            ? Visibility.Visible
            : Visibility.Collapsed;
        FlavorVisibility  = !string.IsNullOrEmpty(FlavorText)
            ? Visibility.Visible
            : Visibility.Collapsed;

        var iconUri = meta.GetIcon(entry.Id);
        if (iconUri is not null)
            _iconSource = new BitmapImage(iconUri);

        QualitySource = new BitmapImage(StaticResources.QualityIcon(entry.Rank));
    }
}
