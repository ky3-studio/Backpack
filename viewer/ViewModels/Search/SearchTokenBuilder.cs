using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Search;

internal sealed class SearchTokenBuilder<TItem>
{
    private readonly IReadOnlyList<TItem> _items;
    private readonly Dictionary<string, SearchToken> _tokens = [];
    private int _order;

    public SearchTokenBuilder(IEnumerable<TItem> items) =>
        _items = items as IReadOnlyList<TItem> ?? [.. items];

    public SearchTokenBuilder<TItem> PerItem(SearchTokenKind kind, Func<TItem, string> value, Func<TItem, Uri?> sideIcon)
    {
        foreach (var item in _items)
        {
            var key = value(item);
            _tokens[key] = new SearchToken(kind, key, _order++, sideIconUri: sideIcon(item));
        }
        return this;
    }

    public SearchTokenBuilder<TItem> GroupedSideIcon(SearchTokenKind kind, Func<TItem, string> key, Func<TItem, Uri?> icon)
    {
        foreach (var group in _items.Where(x => !string.IsNullOrEmpty(key(x))).GroupBy(key))
            _tokens[group.Key] = new SearchToken(kind, group.Key, _order++, sideIconUri: icon(group.First()));
        return this;
    }

    public SearchTokenBuilder<TItem> GroupedMonoIcon(SearchTokenKind kind, Func<TItem, string> key, Func<TItem, Uri?> icon)
    {
        foreach (var group in _items.Where(x => !string.IsNullOrEmpty(key(x))).GroupBy(key))
            _tokens[group.Key] = new SearchToken(kind, group.Key, _order++, iconUri: icon(group.First()));
        return this;
    }

    public SearchTokenBuilder<TItem> Distinct(SearchTokenKind kind, Func<TItem, string> value)
    {
        foreach (var v in _items.Select(value).Where(v => !string.IsNullOrEmpty(v)).Distinct())
            _tokens[v] = new SearchToken(kind, v, _order++);
        return this;
    }

    public SearchTokenBuilder<TItem> WeaponTypes(Func<TItem, string> typeOf)
    {
        foreach (var type in _items.Select(typeOf).Where(t => !string.IsNullOrEmpty(t)).Distinct())
            _tokens[type] = CommonSearchTokens.WeaponType(type, _order++);
        return this;
    }

    public SearchTokenBuilder<TItem> Quality(Func<TItem, int> rankOf)
    {
        foreach (var rank in _items.Select(rankOf).Where(r => r > 0).Distinct().OrderByDescending(r => r))
            _tokens[CommonSearchTokens.RankLabel(rank)] = CommonSearchTokens.Quality(rank, _order++);
        return this;
    }

    public IReadOnlyDictionary<string, SearchToken> Build() => _tokens;
}