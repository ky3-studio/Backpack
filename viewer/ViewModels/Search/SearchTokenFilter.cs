using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Search;

internal static class SearchTokenFilter
{
    public static IReadOnlyList<T>? Apply<T>(IReadOnlyList<T>? items, IReadOnlyCollection<SearchToken> tokens, Func<T, SearchTokenKind, string> matchValue)
    {
        if (items is null) return null;
        if (tokens.Count == 0) return items;

        var result = items.AsEnumerable();
        foreach (var group in tokens.GroupBy(t => t.Kind))
        {
            var kind   = group.Key;
            var values = group.Select(t => t.Value).ToHashSet();
            result = result.Where(item => values.Contains(matchValue(item, kind)));
        }
        return result.ToList();
    }
}
