using Backpack.Viewer;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels;

public sealed class MonsterVariantViewModel
{
    public MonsterVariantViewModel(MonsterMetaService.MonsterVariant source, string label)
    {
        Source  = source;
        Label   = label;
        Affixes = [.. source.Affixes.Select(a => new MonsterAffixViewModel(a.Name, a.Description))];
        Resists = [.. source.Resists.Select(r => new MonsterResistViewModel(r.Element, r.Value))];
        Drops   = [.. source.Drops.Select(d => new MonsterDropViewModel(d))];
    }

    public MonsterMetaService.MonsterVariant       Source  { get; }
    public string                                  Label   { get; }
    public IReadOnlyList<MonsterAffixViewModel>    Affixes { get; }
    public IReadOnlyList<MonsterResistViewModel>   Resists { get; }
    public IReadOnlyList<MonsterDropViewModel>     Drops   { get; }

    public bool HasBaseValue => Source.HasBaseValue;

    public Visibility AffixesVisibility => (Affixes.Count > 0).ToVisibility();
    public Visibility ResistsVisibility => (Resists.Count > 0).ToVisibility();
    public Visibility DropsVisibility   => (Drops.Count   > 0).ToVisibility();
}
