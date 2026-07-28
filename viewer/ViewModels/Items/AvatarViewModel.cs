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

    public AvatarEntry     Source              { get; }
    public WeaponViewModel? Weapon             { get; }
    public string          Name                { get; }
    public string          Element             { get; }
    public int             Rarity              { get; }
    public string          RankDisplay         { get; }
    public string          LevelFull           { get; }
    public string          FetterText          { get; }
    public BitmapImage     QualitySource       { get; }
    public Visibility      HasWeaponVisibility { get; }
    public string          WeaponName          { get; }
    public string          WeaponRankDisplay   { get; }
    public string          WeaponLevelText     { get; }
    public string          WeaponRefineText    { get; }
    public BitmapImage?    WeaponQualitySource => Weapon?.QualitySource;
    public BitmapImage?    WeaponIconSource    => Weapon?.IconSource;

    public IReadOnlyList<int>  PromoteItems       { get; }
    public IReadOnlyList<int>  WeaponPromoteItems { get; }
    public IReadOnlyList<SkillSlotViewModel>  Skills  { get; }
    public IReadOnlyList<TalentSlotViewModel> Talents { get; }

    public AvatarViewModel(AvatarEntry entry, AvatarMetaService avatarMeta, AvatarDetailService? avatarDetail = null, WeaponViewModel? weapon = null)
    {
        Source = entry;
        Weapon = weapon;
        var m  = avatarMeta.GetMeta(entry.Id);

        Name        = m?.Name      ?? entry.Id.ToString();
        Element     = m?.ElementCn ?? string.Empty;
        Rarity      = m?.Rarity    ?? 1;
        RankDisplay = new string('★', Math.Clamp(Rarity, 0, 5));
        LevelFull   = entry.Level > 0 ? $"{Localized.Get("LevelPrefix")}{entry.Level}" : string.Empty;
        FetterText  = entry.Fetter > 0 ? entry.Fetter.ToString() : string.Empty;
        QualitySource = new BitmapImage(StaticResources.QualityIcon(Rarity));

        HasWeaponVisibility = (weapon is not null).ToVisibility();
        WeaponName          = weapon?.Source.Name ?? string.Empty;
        WeaponRankDisplay   = weapon is not null ? new string('★', Math.Clamp(weapon.Source.Rank, 0, 5)) : string.Empty;
        WeaponLevelText     = weapon?.Source.Level > 0 ? $"{Localized.Get("LevelPrefix")}{weapon.Source.Level}" : string.Empty;
        WeaponRefineText    = weapon is not null ? string.Format(Localized.Get("WeaponRefineFmt"), weapon.Source.Refine) : string.Empty;

        PromoteItems       = [.. Enumerable.Range(0, Math.Clamp(entry.Promote, 0, 6))];
        WeaponPromoteItems = weapon is not null ? [.. Enumerable.Range(0, Math.Clamp(weapon.Source.Promote, 0, 6))] : [];

        if (weapon is not null)
        {
            weapon.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(WeaponViewModel.IconSource))
                    OnPropertyChanged(nameof(WeaponIconSource));
            };
        }

        if (m is not null)
        {
            var extraMap = new Dictionary<uint, int>();
            foreach (var e in entry.Extras) if (e.Length >= 2) extraMap[(uint)e[0]] = e[1];
            var skillMap = new Dictionary<uint, int>();
            foreach (var s in entry.Skills) if (s.Length >= 2) skillMap[(uint)s[0]] = s[1];

            Skills = [.. m.Skills.Select(s =>
            {
                var baseLevel = skillMap.GetValueOrDefault(s.Id);
                var extra     = extraMap.GetValueOrDefault(s.GroupId);
                int total     = baseLevel + extra;
                var (desc, skillParams) = avatarDetail is not null
                    ? avatarDetail.GetSkillInfo(entry.Id, s.GroupId, total)
                    : (string.Empty, (IReadOnlyList<SkillParamRow>)[]);
                return new SkillSlotViewModel(s.Name, s.Icon, total, s.Type, desc, skillParams);
            })];

            var skillNames = m.Skills.Select(s => s.Name).ToArray();
            var activeSet  = new HashSet<uint>(entry.Talents);
            Talents = [.. m.Talents.Select(t =>
            {
                string? extraText = t.ExtraLevel is { } el
                    ? $"{(el.Index < skillNames.Length ? skillNames[el.Index] : Localized.Get("SkillFallback"))} +{el.Value} {Localized.Get("ExtraLevelSuffix")}"
                    : null;
                return new TalentSlotViewModel(t.Name, t.Icon, activeSet.Contains(t.Id), t.Description, extraText);
            })];

            GfxLoader.BeginLoad(StaticResources.AvatarIcon(m.Icon), this);
            GfxLoader.BeginLoad(StaticResources.AvatarIcon(m.Icon), new CardIconProxy(v => CardIconSource = v));
            GfxLoader.BeginLoad(StaticResources.AvatarIcon(m.SideIcon), new SideProxy(v => SideIconSource = v));

            if (!string.IsNullOrEmpty(m.Namecard))
                GfxLoader.BeginLoad(StaticResources.AvatarCard(m.Namecard), new CardProxy(v => CardSource = v));
        }
        else
        {
            Skills  = [];
            Talents = [];
        }
    }

    private sealed class CardIconProxy(Action<BitmapImage?> setter) : IIconUpdatable
    {
        public BitmapImage? IconSource { get => null; set => setter(value); }
    }

    private sealed class CardProxy(Action<BitmapImage?> setter) : IIconUpdatable
    {
        public BitmapImage? IconSource { get => null; set => setter(value); }
    }

    private sealed class SideProxy(Action<BitmapImage?> setter) : IIconUpdatable
    {
        public BitmapImage? IconSource { get => null; set => setter(value); }
    }
}
