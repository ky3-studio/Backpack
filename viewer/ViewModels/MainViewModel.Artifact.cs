using System.Collections.ObjectModel;
using System.Text.Json;
using Backpack.Viewer.Models;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<ArtifactSetViewModel> Artifacts { get; } = [];

    private void LoadDefaultArtifacts()
    {
        Artifacts.Clear();
        foreach (var setName in _artifactMeta.GetSetNames())
            Artifacts.Add(new ArtifactSetViewModel(setName, _artifactMeta));
    }

    internal void ApplyArtifact(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.ArtifactEntryArray);
        if (entries is null) return;
        _db.SaveArtifacts(entries);
    }
}
