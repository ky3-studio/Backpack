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

    [ObservableProperty] private IReadOnlyList<SkillSlotViewModel>  _skills  = [];
    [ObservableProperty] private IReadOnlyList<TalentSlotViewModel> _talents = [];

    public BitmapImage? WeaponQualitySource => _weapon?.QualitySource;
    public BitmapImage? WeaponIconSource    => _weapon?.IconSource;

    private WeaponViewModel? _weapon;
    private readonly AvatarMetaService.AvatarMeta? _meta;

    public AvatarViewModel(AvatarEntry entry, AvatarMetaService avatarMeta,
        AvatarDetailService? avatarDetail = null, WeaponViewModel? weapon = null)
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

        ApplyEntry(entry, weapon, avatarDetail);

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
        AvatarDetailService? avatarDetail = null)
    {
        Source = entry;
        ApplyEntry(entry, weapon, avatarDetail);
    }

    private void ApplyEntry(AvatarEntry entry, WeaponViewModel? weapon, AvatarDetailService? avatarDetail)
    {
        int level      = Math.Max(1, entry.Level);
        int friendship = Math.Max(1, entry.Friendship);

        LevelFull    = $"{Localized.Get("LevelPrefix")}{level}";
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
        WeaponLevelText     = weapon?.Source.Level > 0 ? $"{Localized.Get("LevelPrefix")}{weapon.Source.Level}" : string.Empty;
        WeaponRefineText    = weapon is not null ? string.Format(Localized.Get("WeaponRefineFmt"), weapon.Source.Refine) : string.Empty;
        WeaponPromoteItems  = weapon is not null ? [.. Enumerable.Range(0, Math.Clamp(weapon.Source.Ascension, 0, 6))] : [];
        OnPropertyChanged(nameof(WeaponQualitySource));
        OnPropertyChanged(nameof(WeaponIconSource));

        if (_meta is null)
        {
            Skills  = [];
            Talents = [];
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
                ? $"{ExtraLevelSkillName(el.Index)} +{el.Value} {Localized.Get("ExtraLevelSuffix")}"
                : null;
            return new TalentSlotViewModel(t.Name, t.Icon, isActive, t.Description, extraText);
        })];
    }

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
        return string.IsNullOrEmpty(name) ? Localized.Get("SkillFallback") : name;
    }

    private void OnWeaponPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(WeaponViewModel.IconSource))
            OnPropertyChanged(nameof(WeaponIconSource));
    }
}
