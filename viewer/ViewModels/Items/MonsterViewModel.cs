using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MonsterViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LevelText))]
    [NotifyPropertyChangedFor(nameof(HpText))]
    [NotifyPropertyChangedFor(nameof(AtkText))]
    [NotifyPropertyChangedFor(nameof(DefText))]
    [NotifyPropertyChangedFor(nameof(CoopHp))]
    private double _level = 90;

    private readonly MonsterMetaService _meta;
    private readonly MonsterVariantViewModel? _variant;

    public uint   Id          { get; }
    public string Name        { get; }
    public string Title       { get; }
    public string SpecialName { get; }
    public string Description { get; }
    public string TypeName    { get; }
    public Uri?   IconUri     { get; }

    public IReadOnlyList<MonsterTipViewModel> Tips { get; }

    public int MaxLevel => 200;

    public IReadOnlyList<MonsterResistViewModel> Resists => _variant?.Resists ?? [];
    public IReadOnlyList<MonsterDropViewModel>   Drops   => _variant?.Drops   ?? [];

    public Visibility TitleVisibility       => (!string.IsNullOrEmpty(Title) && Title != Name).ToVisibility();
    public Visibility DescriptionVisibility => (!string.IsNullOrEmpty(Description)).ToVisibility();
    public Visibility TipsVisibility        => (Tips.Count > 0).ToVisibility();
    public Visibility ResistsVisibility     => _variant?.ResistsVisibility ?? Visibility.Collapsed;
    public Visibility DropsVisibility       => _variant?.DropsVisibility   ?? Visibility.Collapsed;
    public Visibility StatsVisibility       => (_variant?.HasBaseValue ?? false).ToVisibility();

    public string LevelText => $"{SR.LevelPrefix}{(int)Level}";
    public string HpText  => Stat.Hp  > 0 ? Stat.Hp.ToString("N0")  : SR.StatEmptyValue;
    public string AtkText => Stat.Atk > 0 ? Stat.Atk.ToString("N0") : SR.StatEmptyValue;
    public string DefText => Stat.Def > 0 ? Stat.Def.ToString("N0") : SR.StatEmptyValue;

    public IReadOnlyList<MonsterCoopHp> CoopHp
    {
        get
        {
            var hp = Stat.Hp;
            if (hp <= 0) return [];
            return [.. MonsterMetaService.CoopHpMultipliers.Select((mul, i) =>
                new MonsterCoopHp($"{i + 1}P", ((long)Math.Round(hp * mul)).ToString("N0")))];
        }
    }

    private (int Hp, int Atk, int Def) Stat =>
        _variant is { } v ? _meta.CalcStats(v.Source, (int)Level) : (0, 0, 0);

    public MonsterViewModel(MonsterMetaService.MonsterMeta source, MonsterMetaService meta)
    {
        _meta = meta;

        Id          = source.Id;
        Name        = source.Name;
        Title       = source.Title;
        SpecialName = source.SpecialName;
        Description = source.Description;
        TypeName    = source.Type;
        IconUri     = StaticResources.MonsterIcon(source.Icon);

        _variant = source.Variant is { } v ? new MonsterVariantViewModel(v) : null;
        Tips     = [.. source.Tips.Select(t => new MonsterTipViewModel(t))];

        GfxLoader.BeginLoad(IconUri, this);
    }
}
