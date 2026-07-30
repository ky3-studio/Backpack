using System.Collections.ObjectModel;
using System.Text.Json;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<GroupViewModel<ArtifactViewModel>> ArtifactGroups { get; } = [];

    private static readonly string[] _slotOrder =
    [
        SR.SlotFlower,
        SR.SlotPlume,
        SR.SlotSands,
        SR.SlotGoblet,
        SR.SlotCirclet,
    ];

    private void BuildArtifactGroups(IEnumerable<ArtifactViewModel> items)
    {
        var bySlot = items.GroupBy(vm => vm.Source.Slot)
                         .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtifactViewModel>)g.ToList());
        ArtifactGroups.Clear();
        foreach (var slot in _slotOrder)
        {
            bySlot.TryGetValue(slot, out var slotItems);
            ArtifactGroups.Add(new GroupViewModel<ArtifactViewModel>(slot, slotItems ?? []));
        }
    }

    private void LoadDefaultArtifacts()
    {
        BuildArtifactGroups(_artifactMeta.GetDefaultEntries()
            .Select(e => new ArtifactViewModel(e, _artifactMeta)));
    }

    internal void ApplyArtifact(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.ArtifactEntryArray);
        if (entries is null) return;
        BuildArtifactGroups(entries.Select(e => new ArtifactViewModel(e, _artifactMeta)));
        _db.SaveArtifacts(entries);
    }
}
