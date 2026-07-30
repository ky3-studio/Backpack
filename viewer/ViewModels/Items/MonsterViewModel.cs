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
    private double _level = 90;

    private readonly MonsterMetaService _meta;
    private readonly MonsterMetaService.MonsterMeta _source;
    private (int Hp, int Atk, int Def) _stat;

    public uint   Id          { get; }
    public string Name        { get; }
    public string Title       { get; }
    public string Description { get; }
    public string TypeName    { get; }
    public Uri?   IconUri     { get; }

    public IReadOnlyList<string>                 Affixes { get; }
    public IReadOnlyList<MonsterResistViewModel> Resists { get; }
    public IReadOnlyList<MaterialItemViewModel>  Drops   { get; }

    public int MaxLevel => 100;

    public Visibility TitleVisibility       => (!string.IsNullOrEmpty(Title) && Title != Name).ToVisibility();
    public Visibility DescriptionVisibility => (!string.IsNullOrEmpty(Description)).ToVisibility();
    public Visibility AffixesVisibility     => (Affixes.Count > 0).ToVisibility();
    public Visibility ResistsVisibility     => (Resists.Count > 0).ToVisibility();
    public Visibility DropsVisibility       => (Drops.Count  > 0).ToVisibility();
    public Visibility StatsVisibility       => _source.HasBaseValue.ToVisibility();

    public string LevelText => $"{SR.LevelPrefix}{(int)Level}";
    public string HpText  => _stat.Hp  > 0 ? _stat.Hp.ToString("N0")  : SR.StatEmptyValue;
    public string AtkText => _stat.Atk > 0 ? _stat.Atk.ToString("N0") : SR.StatEmptyValue;
    public string DefText => _stat.Def > 0 ? _stat.Def.ToString("N0") : SR.StatEmptyValue;

    public MonsterViewModel(MonsterMetaService.MonsterMeta source, MonsterMetaService meta, MaterialMetaService materialMeta)
    {
        _source = source;
        _meta   = meta;

        Id          = source.Id;
        Name        = source.Name;
        Title       = source.Title;
        Description = source.Description;
        TypeName    = TypeLabel(source.Type);
        IconUri     = StaticResources.MonsterIcon(source.Icon);
        Affixes     = source.Affixes;
        Resists     = [.. source.Resists.Select(r => new MonsterResistViewModel(r.Element, r.Value))];
        Drops       = [.. source.Drops
            .Select(id => new MaterialItemViewModel((uint)id, materialMeta))
            .Where(d => !string.IsNullOrEmpty(d.Name))];

        _stat = _meta.CalcStats(_source, (int)Level);
        GfxLoader.BeginLoad(IconUri, this);
    }

    partial void OnLevelChanged(double value) =>
        _stat = _meta.CalcStats(_source, (int)value);

    private static string TypeLabel(int type) => type switch
    {
        1 => SR.MonsterTypeOrdinary,
        2 => SR.MonsterTypeBoss,
        _ => SR.MonsterTypeOther,
    };
}
