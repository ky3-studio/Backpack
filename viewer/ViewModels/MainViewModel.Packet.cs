using Backpack.Viewer.Localization;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public event Action? SyncCompleted;

    public void OnPacketReceived(object? _, (string Event, string Json) args)
    {
        var (evt, json) = args;
        _dispatcher.TryEnqueue(() => Apply(evt, json));
    }

    private void Apply(string evt, string json)
    {
        switch (evt)
        {
            case "weapon":   ApplyWeapon(json);   break;
            case "artifact": ApplyArtifact(json); break;
            case "avatar":   ApplyAvatar(json);   break;
            case "material": ApplyMaterial(json); break;
            case "prop":     ApplyProp(json);     break;
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
            foreach (var e in dbWeapons) Weapons.Add(new WeaponViewModel(e, _weaponMeta, _materialMeta));
        else
            LoadDefaultWeapons();
        RebuildWeaponGroups();

        var dbArtifacts = _db.LoadArtifacts();
        if (dbArtifacts.Count > 0)
            BuildArtifactGroups(dbArtifacts.Select(e => new ArtifactViewModel(e, _artifactMeta)));
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
