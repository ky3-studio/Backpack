using System.Text.Json;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public event Action? DataReceived;

    public void OnPacketReceived(object? _, (string Event, string Json) args)
    {
        var (evt, json) = args;
        _dispatcher.TryEnqueue(() => Apply(evt, json));
    }

    private void Apply(string evt, string json)
    {
        switch (evt)
        {
            case "weapon":
            {
                var bag = JsonSerializer.Deserialize<WeaponBag>(json);
                if (bag is null) return;
                Weapons.Clear();
                foreach (var e in bag.Weapons) Weapons.Add(new WeaponViewModel(e, _weaponMeta));
                _db.SaveWeapons(bag.Weapons);
                break;
            }
            case "artifact":
            {
                var bag = JsonSerializer.Deserialize<ArtifactBag>(json);
                if (bag is null) return;
                Artifacts.Clear();
                foreach (var e in bag.Artifacts) Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
                _db.SaveArtifacts(bag.Artifacts);
                break;
            }
            case "avatar":
            {
                var bag = JsonSerializer.Deserialize<AvatarBag>(json);
                if (bag is null) return;
                RebuildAvatars(bag.Avatars);
                _db.SaveAvatars(bag.Avatars);
                break;
            }
            case "material":
            {
                var bag = JsonSerializer.Deserialize<MaterialBag>(json);
                if (bag is null) return;
                foreach (var e in bag.Materials)
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
                var bag = JsonSerializer.Deserialize<PropBag>(json);
                if (bag is null) return;
                foreach (var (k, v) in bag.Props)
                    _activeProps[k] = v;
                _db.SaveProps(_activeProps);
                RebuildAssetGroups();
                return;
            }
        }
        IsLaunching = false;
        StatusText = $"{Localized.Get("StatusReceived")} · {DateTime.Now:HH:mm:ss}";
        DataReceived?.Invoke();
    }
}
