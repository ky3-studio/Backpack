using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class AvatarViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty] private BitmapImage? _iconSource;
    [ObservableProperty] private BitmapImage? _cardIconSource;
    [ObservableProperty] private BitmapImage? _sideIconSource;
    [ObservableProperty] private BitmapImage? _cardSource;

    public AvatarEntry Source         { get; private set; }
    public string      Name           { get; }
    public string      Element        { get; }
    public int         Rarity         { get; }
    public string      WeaponTypeName { get; }
    public Uri?        IconUri        { get; }
    public Uri?        ElementIconUri { get; }
    public IReadOnlyList<int> RankItems { get; }
    public BitmapImage QualitySource  { get; }

    [ObservableProperty] private string _levelFull      = string.Empty;
    [ObservableProperty] private string _fetterText     = string.Empty;
    [ObservableProperty] private IReadOnlyList<int> _promoteItems = [];

    [ObservableProperty] private Visibility _hasWeaponVisibility = Visibility.Collapsed;
    [ObservableProperty] private string _weaponName         = string.Empty;
    [ObservableProperty] private IReadOnlyList<int> _weaponRankItems  = [];
    [ObservableProperty] private string _weaponLevelText    = string.Empty;
    [ObservableProperty] private string _weaponRefineText   = string.Empty;
    [ObservableProperty] private IReadOnlyList<int> _weaponPromoteItems = [];

    [ObservableProperty] private IReadOnlyList<SkillSlotViewModel>  _skills    = [];
    [ObservableProperty] private IReadOnlyList<TalentSlotViewModel> _talents   = [];
    [ObservableProperty] private IReadOnlyList<TalentSlotViewModel> _inherents = [];

    [ObservableProperty] private Visibility _hasArtifactsVisibility = Visibility.Collapsed;
    [ObservableProperty] private IReadOnlyList<OwnedArtifactViewModel> _artifacts = [];

    [ObservableProperty] private IReadOnlyList<AvatarStatRow> _statRows = [];

    public BitmapImage? WeaponQualitySource => _weapon?.QualitySource;
    public BitmapImage? WeaponIconSource    => _weapon?.IconSource;
    public string       StatPanelTitle      => SR.StatPanelTitle;

    private WeaponViewModel? _weapon;
    private readonly AvatarMetaService.AvatarMeta? _meta;

    public AvatarViewModel(AvatarEntry entry, AvatarMetaService avatarMeta,
        AvatarDetailService? avatarDetail = null, WeaponViewModel? weapon = null,
        IReadOnlyList<OwnedArtifactViewModel>? artifacts = null)
    {
        Source = entry;
        _meta  = avatarMeta.GetMeta(entry.Id);

        Name          = _meta?.Name      ?? entry.Name    ?? entry.Id.ToString();
        Element       = _meta?.ElementCn  ?? entry.Element ?? string.Empty;
        Rarity        = _meta?.Rarity     ?? Math.Max(1, entry.Rarity);
        WeaponTypeName = _meta is not null ? WeaponTypes.FromRaw(_meta.WeaponType) : string.Empty;
        ElementIconUri = StaticResources.ElementIcon(_meta?.Element);
        IconUri        = _meta is not null ? StaticResources.AvatarIcon(_meta.Icon) : null;
        RankItems     = [.. Enumerable.Range(0, Math.Clamp(Rarity, 0, 5))];
        QualitySource = StaticResources.GetQualityBitmap(Rarity);

        ApplyEntry(entry, weapon, avatarDetail, artifacts);

        if (_meta is not null)
        {
            GfxLoader.BeginLoad(StaticResources.AvatarIcon(_meta.Icon), this);
            GfxLoader.BeginLoad(StaticResources.AvatarIcon(_meta.Icon), new IconSink(v => CardIconSource = v));
            GfxLoader.BeginLoad(StaticResources.AvatarIcon(_meta.SideIcon), new IconSink(v => SideIconSource = v));
            if (!string.IsNullOrEmpty(_meta.Namecard))
                GfxLoader.BeginLoad(StaticResources.AvatarCard(_meta.Namecard), new IconSink(v => CardSource = v));
        }
    }

    public void Update(AvatarEntry entry, WeaponViewModel? weapon = null,
        AvatarDetailService? avatarDetail = null,
        IReadOnlyList<OwnedArtifactViewModel>? artifacts = null)
    {
        Source = entry;
        ApplyEntry(entry, weapon, avatarDetail, artifacts);
    }

    private void ApplyEntry(AvatarEntry entry, WeaponViewModel? weapon, AvatarDetailService? avatarDetail,
        IReadOnlyList<OwnedArtifactViewModel>? artifacts)
    {
        int level      = Math.Max(1, entry.Level);
        int friendship = Math.Max(1, entry.Friendship);

        LevelFull    = $"{SR.LevelPrefix}{level}";
        FetterText   = friendship.ToString();
        PromoteItems = [.. Enumerable.Range(0, Math.Clamp(entry.Ascension, 0, 6))];

        if (!ReferenceEquals(_weapon, weapon))
        {
            if (_weapon is not null)
                _weapon.PropertyChanged -= OnWeaponPropertyChanged;
            _weapon = weapon;
            if (_weapon is not null)
                _weapon.PropertyChanged += OnWeaponPropertyChanged;
        }

        HasWeaponVisibility = (weapon is not null).ToVisibility();
        WeaponName          = weapon?.Source.Name ?? string.Empty;
        WeaponRankItems    = weapon is not null ? [.. Enumerable.Range(0, Math.Clamp(weapon.Source.Rank, 0, 5))] : [];
        WeaponLevelText     = weapon?.Source.Level > 0 ? $"{SR.LevelPrefix}{weapon.Source.Level}" : string.Empty;
        WeaponRefineText    = weapon is not null ? string.Format(SR.WeaponRefineFmt, weapon.Source.Refine) : string.Empty;
        WeaponPromoteItems  = weapon is not null ? [.. Enumerable.Range(0, Math.Clamp(weapon.Source.Ascension, 0, 6))] : [];
        OnPropertyChanged(nameof(WeaponQualitySource));
        OnPropertyChanged(nameof(WeaponIconSource));

        Artifacts              = artifacts is { Count: > 0 }
            ? [.. artifacts.OrderBy(a => ArtifactSlotRank(a.SlotName))]
            : [];
        HasArtifactsVisibility = (Artifacts.Count > 0).ToVisibility();

        BuildStats(entry);

        if (_meta is null)
        {
            Skills    = [];
            Talents   = [];
            Inherents = [];
            return;
        }

        var extraMap = new Dictionary<uint, int>();
        foreach (var e in entry.Passives) extraMap[e.Id] = e.Extra;
        var skillMap = new Dictionary<uint, int>();
        foreach (var s in entry.Skills) skillMap[s.Id] = s.Level;

        Skills = [.. _meta.Skills.Select(s =>
        {
            var baseLevel = skillMap.GetValueOrDefault(s.Id);
            var extra     = extraMap.GetValueOrDefault(s.GroupId);
            int total     = Math.Max(1, baseLevel + extra);
            var (desc, skillParams) = avatarDetail is not null
                ? avatarDetail.GetSkillInfo(entry.Id, s.GroupId, total)
                : (string.Empty, (IReadOnlyList<SkillParamRow>)[]);
            return new SkillSlotViewModel(s.Name, s.Icon, total, s.Type, desc, skillParams);
        })];

        Talents = [.. _meta.Talents.Select((t, idx) =>
        {
            bool isActive = idx < entry.Constellation;
            string? extraText = t.ExtraLevel is { } el
                ? $"{ExtraLevelSkillName(el.Index)} +{el.Value} {SR.ExtraLevelSuffix}"
                : null;
            return new TalentSlotViewModel(t.Name, t.Icon, isActive, t.Description, extraText);
        })];

        var detailInherents = avatarDetail?.GetInherents(entry.Id) ?? [];
        if (detailInherents.Count > 0)
        {
            var phaseById = _meta.Inherents.ToDictionary(m => m.Id, m => m.UnlockPhase);
            Inherents = [.. detailInherents.Select(i =>
                new TalentSlotViewModel(i.Name, i.Icon, entry.Ascension >= phaseById.GetValueOrDefault(i.Id), i.Description, null))];
        }
        else
        {
            Inherents = [.. _meta.Inherents.Select(i =>
                new TalentSlotViewModel(i.Name, i.Icon, entry.Ascension >= i.UnlockPhase, i.Description, null))];
        }
    }

    private void BuildStats(AvatarEntry entry)
    {
        var fp = entry.FightProps;
        bool hasData = fp is { Count: > 0 };

        float Get(int k) => hasData && fp!.TryGetValue(k.ToString(), out var v) ? v : 0f;
        float Res(int k) => hasData ? 0.15f + Get(k) : 0f;
        static string Pct(float v) => (v * 100f).ToString("0.0") + "%";
        static string Whole(float v) => Math.Round(v).ToString("0");
        static string? Green(float total, float baseVal)
        {
            int g = (int)Math.Round(total) - (int)Math.Round(baseVal);
            return g > 0 ? g.ToString() : null;
        }

        var rows = new List<AvatarStatRow>
        {
            new("HP",              SR.StatHp,             Whole(Get(2000)), Green(Get(2000), Get(1))),
            new("CRITICAL",        SR.StatCritRate,       Pct(Get(20))),
            new("ATTACK",          SR.StatAtk,            Whole(Get(2001)), Green(Get(2001), Get(4))),
            new("CRITICAL_HURT",   SR.StatCritDmg,        Pct(Get(22))),
            new("DEFENSE",         SR.StatDef,            Whole(Get(2002)), Green(Get(2002), Get(7))),
            new("HEAL_ADD",        SR.StatHealingBonus,   Pct(Get(26))),
            new("ELEMENT_MASTERY", SR.StatElementMastery, Whole(Get(28))),
            new("HEALED_ADD",      SR.StatIncomingHealing,Pct(Get(27))),
        };

        int charKey = CharElementKey();
        var elemDmg = new (int key, string icon, string label)[]
        {
            (40, "PYRO",    SR.StatPyroDmg),
            (41, "ELECTRO", SR.StatElectroDmg),
            (42, "HYDRO",   SR.StatHydroDmg),
            (43, "DENDRO",  SR.StatDendroDmg),
            (44, "ANEMO",   SR.StatAnemoDmg),
            (45, "GEO",     SR.StatGeoDmg),
            (46, "CRYO",    SR.StatCryoDmg),
        };
        foreach (var (key, icon, label) in elemDmg)
            if (key == charKey || Get(key) > 0)
                rows.Add(new(icon, label, Pct(Get(key))));
        if (Get(30) > 0)
            rows.Add(new("PHYSICAL_ADD_HURT", SR.StatPhysicalDmg, Pct(Get(30))));

        rows.Add(new("CHARGE_EFFICIENCY", SR.StatEnergyRecharge, Pct(Get(23))));
        if (Get(80) > 0)
            rows.Add(new(null, SR.StatCdReduction, Pct(Get(80))));
        if (Get(81) > 0)
            rows.Add(new("SHIELD_COST_MINUS_RATIO", SR.StatShieldStrength, Pct(Get(81))));

        rows.Add(new("PHYSICAL_ADD_HURT", SR.StatPhysicalRes, Pct(Res(29))));
        rows.Add(new("PYRO",    SR.StatPyroRes,    Pct(Res(50))));
        rows.Add(new("ELECTRO", SR.StatElectroRes, Pct(Res(51))));
        rows.Add(new("HYDRO",   SR.StatHydroRes,   Pct(Res(52))));
        rows.Add(new("DENDRO",  SR.StatDendroRes,  Pct(Res(53))));
        rows.Add(new("ANEMO",   SR.StatAnemoRes,   Pct(Res(54))));
        rows.Add(new("GEO",     SR.StatGeoRes,     Pct(Res(55))));
        rows.Add(new("CRYO",    SR.StatCryoRes,    Pct(Res(56))));

        StatRows = rows;
    }

    private int CharElementKey() => _meta?.Element?.ToUpperInvariant() switch
    {
        "PYRO"    => 40,
        "ELECTRO" => 41,
        "HYDRO"   => 42,
        "DENDRO"  => 43,
        "ANEMO"   => 44,
        "GEO"     => 45,
        "CRYO"    => 46,
        _         => 0,
    };

    private string ExtraLevelSkillName(int slot)
    {
        var type = slot switch
        {
            1 => "normal",
            2 => "skill",
            9 => "burst",
            _ => null,
        };
        var name = type is not null ? _meta?.Skills.FirstOrDefault(s => s.Type == type)?.Name : null;
        return string.IsNullOrEmpty(name) ? SR.SkillFallback : name;
    }

    private static int ArtifactSlotRank(string slot)
    {
        if (slot == SR.SlotFlower)  return 0;
        if (slot == SR.SlotPlume)   return 1;
        if (slot == SR.SlotSands)   return 2;
        if (slot == SR.SlotGoblet)  return 3;
        if (slot == SR.SlotCirclet) return 4;
        return 5;
    }

    private void OnWeaponPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(WeaponViewModel.IconSource))
            OnPropertyChanged(nameof(WeaponIconSource));
    }
}
