using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal static class WeaponSearchTokens
{
    public static string RankLabel(int rank)
    {
        return Math.Clamp(rank, 1, 5).ToString();
    }

    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<WeaponViewModel> weapons)
    {
        IReadOnlyList<WeaponViewModel> list = weapons as IReadOnlyList<WeaponViewModel> ?? [.. weapons];
        Dictionary<string, SearchToken> tokens = [];
        int order = 0;

        foreach (var vm in list)
        {
            tokens[vm.Source.Name] = new SearchToken(SearchTokenKind.Weapon, vm.Source.Name, order++, sideIconUri: vm.IconUri);
        }

        foreach (var type in list.Select(vm => vm.Source.Type).Where(t => !string.IsNullOrEmpty(t)).Distinct())
        {
            tokens[type] = new SearchToken(SearchTokenKind.WeaponType, type, order++);
        }

        foreach (var rank in list.Select(vm => vm.Source.Rank).Where(r => r > 0).Distinct().OrderByDescending(r => r))
        {
            var label = RankLabel(rank);
            tokens[label] = new SearchToken(SearchTokenKind.ItemQuality, label, order++, iconUri: StaticResources.RankStarsIcon(rank), showText: false);
        }

        foreach (var sub in list.Select(vm => vm.SubPropName).Where(s => !string.IsNullOrEmpty(s)).Distinct())
        {
            tokens[sub] = new SearchToken(SearchTokenKind.FightProperty, sub, order++);
        }

        return tokens;
    }
}
