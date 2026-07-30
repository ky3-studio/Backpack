using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Search;

internal static class ItemSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<ISearchableItem> items)
    {
        var list = items as IReadOnlyList<ISearchableItem> ?? [.. items];
        var tokens = new Dictionary<string, SearchToken>();
        int order = 0;

        foreach (var group in list.Where(vm => !string.IsNullOrEmpty(vm.Name)).GroupBy(vm => vm.Name))
            tokens[group.Key] = new SearchToken(SearchTokenKind.Material, group.Key, order++, sideIconUri: group.First().IconUri);

        foreach (var rank in list.Select(vm => vm.Rank).Where(r => r > 0).Distinct().OrderByDescending(r => r))
            tokens[CommonSearchTokens.RankLabel(rank)] = CommonSearchTokens.Quality(rank, order++);

        return tokens;
    }

    public static string MatchValue(ISearchableItem vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.Material    => vm.Name,
        SearchTokenKind.ItemQuality => CommonSearchTokens.RankLabel(vm.Rank),
        _                           => string.Empty,
    };
}
