using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Artifact;

internal static class ArtifactSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<ArtifactViewModel> artifacts)
    {
        var list = artifacts as IReadOnlyList<ArtifactViewModel> ?? [.. artifacts];
        var tokens = new Dictionary<string, SearchToken>();
        int order = 0;

        foreach (var group in list.Where(vm => !string.IsNullOrEmpty(vm.Source.Set)).GroupBy(vm => vm.Source.Set))
            tokens[group.Key] = new SearchToken(SearchTokenKind.ArtifactSet, group.Key, order++, sideIconUri: group.First().IconUri);

        foreach (var slot in list.Select(vm => vm.Source.Slot).Where(s => !string.IsNullOrEmpty(s)).Distinct())
            tokens[slot] = new SearchToken(SearchTokenKind.ArtifactSlot, slot, order++);

        foreach (var rank in list.Select(vm => vm.Source.Rank).Where(r => r > 0).Distinct().OrderByDescending(r => r))
            tokens[CommonSearchTokens.RankLabel(rank)] = CommonSearchTokens.Quality(rank, order++);

        return tokens;
    }
}
