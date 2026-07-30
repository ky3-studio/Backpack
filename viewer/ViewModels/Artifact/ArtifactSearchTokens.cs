using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Artifact;

internal static class ArtifactSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<ArtifactSetViewModel> sets) =>
        new SearchTokenBuilder<ArtifactSetViewModel>(sets)
            .PerItem(SearchTokenKind.ArtifactSet, vm => vm.SetName, vm => vm.IconUri)
            .Quality(vm => vm.Rank)
            .Build();
}
