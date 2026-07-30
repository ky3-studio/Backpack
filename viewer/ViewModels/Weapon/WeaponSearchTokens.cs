using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Weapon;

internal static class WeaponSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<WeaponViewModel> weapons)
    {
        var list = weapons as IReadOnlyList<WeaponViewModel> ?? [.. weapons];
        var tokens = new Dictionary<string, SearchToken>();
        int order = 0;

        foreach (var vm in list)
            tokens[vm.Source.Name] = new SearchToken(SearchTokenKind.Weapon, vm.Source.Name, order++, sideIconUri: vm.IconUri);

        foreach (var type in list.Select(vm => vm.Source.Type).Where(t => !string.IsNullOrEmpty(t)).Distinct())
            tokens[type] = CommonSearchTokens.WeaponType(type, order++);

        foreach (var rank in list.Select(vm => vm.Source.Rank).Where(r => r > 0).Distinct().OrderByDescending(r => r))
            tokens[CommonSearchTokens.RankLabel(rank)] = CommonSearchTokens.Quality(rank, order++);

        foreach (var group in list.Where(vm => !string.IsNullOrEmpty(vm.SubPropName)).GroupBy(vm => vm.SubPropName))
            tokens[group.Key] = new SearchToken(SearchTokenKind.FightProperty, group.Key, order++, iconUri: group.First().SubPropIconUri);

        return tokens;
    }
}
