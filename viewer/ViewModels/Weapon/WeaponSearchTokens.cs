using Backpack.Viewer.Services;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Weapon;

internal static class WeaponSearchTokens
{
    public static string RankLabel(int rank)
    {
        return Math.Clamp(rank, 1, 5).ToString();
    }

    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<WeaponViewModel> weapons)
    {
        var list = weapons as IReadOnlyList<WeaponViewModel> ?? [.. weapons];
        var tokens = new Dictionary<string, SearchToken>();
        int order = 0;

        foreach (var vm in list)
            tokens[vm.Source.Name] = new SearchToken(SearchTokenKind.Weapon, vm.Source.Name, order++, sideIconUri: vm.IconUri);

        foreach (var type in list.Select(vm => vm.Source.Type).Where(t => !string.IsNullOrEmpty(t)).Distinct())
            tokens[type] = new SearchToken(SearchTokenKind.WeaponType, type, order++);

        foreach (var rank in list.Select(vm => vm.Source.Rank).Where(r => r > 0).Distinct().OrderByDescending(r => r))
            tokens[RankLabel(rank)] = new SearchToken(SearchTokenKind.ItemQuality, RankLabel(rank), order++, iconUri: StaticResources.RankStarsIcon(rank), showText: false);

        foreach (var subProp in list.Select(vm => vm.SubPropName).Where(p => !string.IsNullOrEmpty(p)).Distinct())
            tokens[subProp] = new SearchToken(SearchTokenKind.FightProperty, subProp, order++);

        return tokens;
    }
}
