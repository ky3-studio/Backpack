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

        Avatars.Clear();
        foreach (var m in _avatarMeta.GetDefaultEntries())
        {
            if (hasData && !map.ContainsKey(m.Id)) continue;
            var entry = map.TryGetValue(m.Id, out var real)
                ? real
                : new AvatarEntry(m.Id, null, null, 0, 0, 0, 0, 0, [], [], []);
            Avatars.Add(ToAvatarViewModel(entry));
        }
    }

    private AvatarViewModel ToAvatarViewModel(AvatarEntry e)
    {
        var weaponSet = new HashSet<string>(Weapons.Select(w => w.Source.Guid));
        var wGuid     = e.Equips.FirstOrDefault(g => weaponSet.Contains(g)) ?? string.Empty;
        var weapon    = !string.IsNullOrEmpty(wGuid)
            ? Weapons.FirstOrDefault(w => w.Source.Guid == wGuid)
            : null;
        return new AvatarViewModel(e, _avatarMeta, _avatarDetail, weapon);
    }

    internal void ApplyAvatar(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.AvatarEntryArray);
        if (entries is null) return;
        RebuildAvatars(entries);
        _db.SaveAvatars(entries);
    }
}
