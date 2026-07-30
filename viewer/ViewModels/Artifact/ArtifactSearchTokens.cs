using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Artifact;

internal static class ArtifactSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<ArtifactViewModel> artifacts) =>
        new SearchTokenBuilder<ArtifactViewModel>(artifacts)
            .GroupedSideIcon(SearchTokenKind.ArtifactSet, vm => vm.Source.Set, vm => vm.IconUri)
            .Distinct(SearchTokenKind.ArtifactSlot, vm => vm.Source.Slot)
            .Quality(vm => vm.Source.Rank)
            .Build();
}