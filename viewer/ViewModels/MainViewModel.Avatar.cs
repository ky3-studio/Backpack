using System.Collections.ObjectModel;
using System.Text.Json;
using Backpack.Viewer.Models;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<AvatarViewModel> Avatars { get; } = [];

    internal void RebuildAvatars(IReadOnlyList<AvatarEntry> realData)
    {
        var map = new Dictionary<uint, AvatarEntry>();
        foreach (var e in realData) map[e.Id] = e;

        bool hasData = map.Count > 0;

        var weaponByGuid = new Dictionary<string, WeaponViewModel>();
        foreach (var w in Weapons)
            if (!string.IsNullOrEmpty(w.Source.Guid))
                weaponByGuid[w.Source.Guid] = w;

        var artifactByGuid = new Dictionary<string, OwnedArtifactViewModel>();
        foreach (var set in Artifacts)
            foreach (var piece in set.OwnedPieces)
                if (!string.IsNullOrEmpty(piece.Source.Guid))
                    artifactByGuid[piece.Source.Guid] = piece;

        Avatars.Clear();
        foreach (var m in _avatarMeta.GetDefaultEntries())
        {
            if (hasData && !map.ContainsKey(m.Id)) continue;
            var entry = map.TryGetValue(m.Id, out var real)
                ? real
                : new AvatarEntry(m.Id, null, null, 0, 0, 0, 0, 0, [], [], []);
            Avatars.Add(ToAvatarViewModel(entry, weaponByGuid, artifactByGuid));
        }
    }

    private AvatarViewModel ToAvatarViewModel(AvatarEntry e,
        Dictionary<string, WeaponViewModel> weaponByGuid,
        Dictionary<string, OwnedArtifactViewModel> artifactByGuid)
    {
        WeaponViewModel? weapon = null;
        var artifacts = new List<OwnedArtifactViewModel>();
        foreach (var g in e.Equips)
        {
            if (weapon is null && weaponByGuid.TryGetValue(g, out var w))
                weapon = w;
            else if (artifactByGuid.TryGetValue(g, out var a))
                artifacts.Add(a);
        }
        return new AvatarViewModel(e, _avatarMeta, _avatarDetail, weapon, artifacts);
    }

    internal void ApplyAvatar(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.AvatarEntryArray);
        if (entries is null) return;
        RebuildAvatars(entries);
        _db.SaveAvatars(entries);
    }
}
