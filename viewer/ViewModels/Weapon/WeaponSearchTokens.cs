using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Weapon;

internal static class WeaponSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<WeaponViewModel> weapons) =>
        new SearchTokenBuilder<WeaponViewModel>(weapons)
            .PerItem(SearchTokenKind.Weapon, vm => vm.Source.Name, vm => vm.IconUri)
            .WeaponTypes(vm => vm.Source.Type)
            .Quality(vm => vm.Source.Rank)
            .GroupedMonoIcon(SearchTokenKind.FightProperty, vm => vm.SubPropName, vm => vm.SubPropIconUri)
            .Build();
}