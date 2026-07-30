using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Avatar;

internal static class AvatarSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<AvatarViewModel> avatars)
    {
        var list = avatars as IReadOnlyList<AvatarViewModel> ?? [.. avatars];
        var tokens = new Dictionary<string, SearchToken>();
        int order = 0;

        foreach (var vm in list)
            tokens[vm.Name] = new SearchToken(SearchTokenKind.Avatar, vm.Name, order++, sideIconUri: vm.IconUri);

        foreach (var group in list.Where(vm => !string.IsNullOrEmpty(vm.Element)).GroupBy(vm => vm.Element))
            tokens[group.Key] = new SearchToken(SearchTokenKind.ElementName, group.Key, order++, iconUri: group.First().ElementIconUri);

        foreach (var type in list.Select(vm => vm.WeaponTypeName).Where(t => !string.IsNullOrEmpty(t)).Distinct())
            tokens[type] = CommonSearchTokens.WeaponType(type, order++);

        foreach (var rarity in list.Select(vm => vm.Rarity).Where(r => r > 0).Distinct().OrderByDescending(r => r))
            tokens[CommonSearchTokens.RankLabel(rarity)] = CommonSearchTokens.Quality(rarity, order++);

        return tokens;
    }
}
