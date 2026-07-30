using System.Collections.ObjectModel;
using System.Text.Json;
using Backpack.Viewer.Models;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<ArtifactSetViewModel> Artifacts { get; } = [];

    internal void BuildArtifacts(IReadOnlyList<ArtifactEntry> owned)
    {
        var ownedBySet = owned
            .Where(e => !string.IsNullOrEmpty(e.Guid))
            .GroupBy(e => e.Set)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtifactEntry>)g.ToList(), StringComparer.OrdinalIgnoreCase);

        Artifacts.Clear();
        foreach (var setName in _artifactMeta.GetSetNames())
        {
            if (ownedBySet.Count > 0 && !ownedBySet.ContainsKey(setName)) continue;
            var vm = new ArtifactSetViewModel(setName, _artifactMeta);
            if (ownedBySet.TryGetValue(setName, out var pieces))
                vm.AttachOwned(pieces);
            Artifacts.Add(vm);
        }
    }

    internal void ApplyArtifact(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.ArtifactEntryArray);
        if (entries is null) return;
        _db.SaveArtifacts(entries);
        BuildArtifacts(entries);
    }
}
