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

    [ObservableProperty]
    private BitmapImage? _typeIconSource;

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
    public string       Description           { get; }
    public string       SubPropName           { get; }
    public Uri?         IconUri               { get; }
    public Uri?         SubPropIconUri        { get; }
    public BitmapImage? SubPropIcon           { get; }
    public Visibility   SubPropIconVisibility => SubPropIcon is not null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility   HasInstanceVisibility { get; }
    public Visibility   CatalogVisibility     { get; }
    public Visibility   SubVisibility         { get; }
    public Visibility   PassiveVisibility     { get; }
    public BitmapImage  QualitySource         { get; }
    public IReadOnlyList<string> RefinementDescriptions { get; }
    public IReadOnlyList<MaterialItemViewModel> CultivationMaterials { get; }
    public Visibility   CultivationVisibility => CultivationMaterials.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public WeaponViewModel(WeaponEntry entry, WeaponMetaService meta, MaterialMetaService materialMeta)
    {
        Source      = entry;
        RankItems   = [.. Enumerable.Range(0, Math.Clamp(entry.Rank, 0, 5))];

        var hasInstance = !string.IsNullOrEmpty(entry.Guid);
        HasInstanceVisibility = hasInstance.ToVisibility();
        CatalogVisibility     = (!hasInstance).ToVisibility();

        if (hasInstance)
        {
            Level       = $"{SR.LevelPrefix}{entry.Level}";
            LevelFull   = $"{SR.LevelPrefix}{entry.Level}/{MaxLevelByPromote[Math.Clamp(entry.Ascension, 0, 6)]}";
            RefineLabel = string.Format(SR.WeaponRefineFmt, entry.Refine);
            Refine      = $"R{entry.Refine}";
            var (atk, sub)     = meta.CalcStats(entry.Id, entry.Level, entry.Ascension);
            var (pName, pDesc) = meta.GetSkill(entry.Id, entry.Refine);
            AtkDisplay         = atk > 0 ? atk.ToString() : string.Empty;
            SubDisplay         = sub;
            PassiveName        = pName;
            SkillDesc          = pDesc;
        }
        else
        {
            Level = Refine = LevelFull = RefineLabel = AtkDisplay = SubDisplay = string.Empty;
            PassiveName = meta.GetSkill(entry.Id, 1).Name;
            SkillDesc   = string.Empty;
        }

        SubVisibility     = (hasInstance && !string.IsNullOrEmpty(SubDisplay)).ToVisibility();
        PassiveVisibility = (!string.IsNullOrEmpty(PassiveName)).ToVisibility();

        Description = meta.GetDescription(entry.Id);
        SubPropName    = meta.GetSubPropName(entry.Id);
        SubPropIconUri = StaticResources.FightPropIcon(meta.GetSubProp(entry.Id));
        SubPropIcon    = StaticResources.FightPropBitmap(meta.GetSubProp(entry.Id));

        RefinementDescriptions = BuildRefinements(entry.Id, meta);

        CultivationMaterials = [.. meta.GetCultivationItemIds(entry.Id).Select(id => new MaterialItemViewModel(id, materialMeta))];

        IconUri = meta.GetIcon(entry.Id);
        if (IconUri is not null)
            GfxLoader.BeginLoad(IconUri, this);

        if (TypeIconName(entry.Type) is { } typeIcon)
            GfxLoader.BeginLoad(StaticResources.SkillIcon(typeIcon), new IconSink(b => TypeIconSource = b));

        QualitySource = StaticResources.GetQualityBitmap(entry.Rank);
    }

    private static IReadOnlyList<string> BuildRefinements(uint id, WeaponMetaService meta)
    {
        List<string> result = [];
        for (var r = 1; r <= 5; r++)
        {
            var desc = meta.GetSkill(id, r).Desc;
            if (string.IsNullOrEmpty(desc)) break;
            result.Add(desc);
        }
        return result;
    }

    public static string? TypeIconName(string type) => type switch
    {
        WeaponTypes.Sword    => "Skill_A_01",
        WeaponTypes.Bow      => "Skill_A_02",
        WeaponTypes.Polearm  => "Skill_A_03",
        WeaponTypes.Claymore => "Skill_A_04",
        WeaponTypes.Catalyst => "Skill_A_Catalyst_MD",
        _                    => null,
    };
}
