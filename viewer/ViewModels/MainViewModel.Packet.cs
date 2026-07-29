using Backpack.Viewer.Localization;
using System.Text.Json;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public event Action? SyncCompleted;

    public void OnPacketReceived(object? _, (string Event, string Json) args)
    {
        var (evt, json) = args;
        _dispatcher.TryEnqueue(() => Apply(evt, json));
    }

    private static uint PropKeyToId(string key) => key switch
    {
        "playerLevel"    => 10013,
        "primogem"       => 10015,
        "mora"           => 10016,
        "worldLevel"     => 10019,
        "resin"          => 10020,
        "genesisCrystal" => 10025,
        "legendaryKey"   => 10027,
        "homeCoin"       => 10042,
        "toyToken"       => 10053,
        "qiyuCoin"       => 10058,
        "reshowCrystal"  => 10069,
        _                => 0
    };

    private void Apply(string evt, string json)
    {
        switch (evt)
        {
            case "weapon":
            {
                var entries = JsonSerializer.Deserialize<WeaponEntry[]>(json);
                if (entries is null) return;
                Weapons.Clear();
                foreach (var e in entries) Weapons.Add(new WeaponViewModel(e, _weaponMeta));
                RebuildWeaponGroups();
                _db.SaveWeapons(entries);
                break;
            }
            case "artifact":
            {
                var entries = JsonSerializer.Deserialize<ArtifactEntry[]>(json);
                if (entries is null) return;
                Artifacts.Clear();
                foreach (var e in entries) Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
                _db.SaveArtifacts(entries);
                break;
            }
            case "avatar":
            {
                var entries = JsonSerializer.Deserialize<AvatarEntry[]>(json);
                if (entries is null) return;
                RebuildAvatars(entries);
                _db.SaveAvatars(entries);
                break;
            }
            case "material":
            {
                var entries = JsonSerializer.Deserialize<MaterialEntry[]>(json);
                if (entries is null) return;
                foreach (var e in entries)
                    _activeCounts[e.Id] = e.Count;
                _db.SaveMaterials(_activeCounts);
                RebuildMaterialGroups();
                RebuildFoodGroups();
                RebuildGadgetGroups();
                RebuildAssetGroups();
                break;
            }
            case "prop":
            {
                using var doc = JsonDocument.Parse(json);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    uint id = PropKeyToId(prop.Name);
                    if (id != 0) _activeProps[id] = prop.Value.GetInt64();
                }
                _db.SaveProps(_activeProps);
                RebuildAssetGroups();
                break;
            }
            case "finish":
                IsLaunching = false;
                StatusText  = $"{Localized.Get("StatusReceived")} · {DateTime.Now:HH:mm:ss}";
                SyncCompleted?.Invoke();
                break;
        }
    }

    internal void Reload()
    {
        var dbWeapons = _db.LoadWeapons();
        Weapons.Clear();
        if (dbWeapons.Count > 0)
            foreach (var e in dbWeapons) Weapons.Add(new WeaponViewModel(e, _weaponMeta));
        else
            LoadDefaultWeapons();
        RebuildWeaponGroups();

        var dbArtifacts = _db.LoadArtifacts();
        Artifacts.Clear();
        if (dbArtifacts.Count > 0)
            foreach (var e in dbArtifacts) Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
        else
            LoadDefaultArtifacts();

        RebuildAvatars(_db.LoadAvatars());

        _activeCounts.Clear();
        foreach (var (k, v) in _db.LoadMaterialCounts()) _activeCounts[k] = v;
        _activeProps.Clear();
        foreach (var (k, v) in _db.LoadProps()) _activeProps[k] = v;
        RebuildMaterialGroups();
        RebuildFoodGroups();
        RebuildGadgetGroups();
        RebuildAssetGroups();
    }
}
