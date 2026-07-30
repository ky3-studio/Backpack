using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Monster;

internal static class MonsterSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<MonsterViewModel> monsters) =>
        new SearchTokenBuilder<MonsterViewModel>(monsters)
            .PerItem(SearchTokenKind.Monster, vm => vm.Name, vm => vm.IconUri)
            .Distinct(SearchTokenKind.MonsterType, vm => vm.TypeName)
            .Build();
}
