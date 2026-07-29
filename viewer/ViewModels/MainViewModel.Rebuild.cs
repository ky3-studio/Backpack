using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    private static readonly (string DataType, string LocaleKey)[] WeaponTypeOrder =
    [
        (WeaponTypes.Sword,    "WeaponTypeSword"),
        (WeaponTypes.Claymore, "WeaponTypeClaymore"),
        (WeaponTypes.Polearm,  "WeaponTypePolearm"),
        (WeaponTypes.Catalyst, "WeaponTypeCatalyst"),
        (WeaponTypes.Bow,      "WeaponTypeBow"),
    ];

    private void RebuildWeaponGroups()
    {
        var byType = Weapons
            .GroupBy(w => w.Source.Type)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<WeaponViewModel>)g.ToList());

        WeaponGroups.Clear();
        foreach (var (dataType, localeKey) in WeaponTypeOrder)
        {
            if (byType.TryGetValue(dataType, out var items))
                WeaponGroups.Add(new GroupViewModel<WeaponViewModel>(Localized.Get(localeKey), items));
        }
        var knownTypes = WeaponTypeOrder.Select(t => t.DataType).ToHashSet();
        foreach (var (dataType, items) in byType)
            if (!knownTypes.Contains(dataType))
                WeaponGroups.Add(new GroupViewModel<WeaponViewModel>(dataType, items));
    }

    private void RebuildMaterialGroups()
    {
        MaterialGroups.Clear();
        foreach (var (label, ids) in _materialMeta.Groups)
        {
            MaterialGroups.Add(new GroupViewModel<MaterialViewModel>(
                label,
                [.. ids.Select(id =>
                {
                    var count = _activeCounts.TryGetValue(id, out var c) ? c : 0UL;
                    var entry = new MaterialEntry(id, _materialMeta.GetName(id), string.Empty, count);
                    return new MaterialViewModel(entry, _materialMeta);
                })]));
        }
    }

    private void RebuildGadgetGroups()
    {
        GadgetGroups.Clear();
        foreach (var (label, ids) in _gadgetMeta.Groups)
        {
            GadgetGroups.Add(new GroupViewModel<GadgetViewModel>(
                label,
                [.. ids.Select(id =>
                {
                    var count = _activeCounts.TryGetValue(id, out var c) ? c : 0UL;
                    var entry = new MaterialEntry(id, _gadgetMeta.GetName(id), string.Empty, count);
                    return new GadgetViewModel(entry, _gadgetMeta);
                })]));
        }
    }

    private void RebuildAssetGroups()
    {
        AssetGroups.Clear();
        foreach (var (label, ids) in _assetMeta.Groups)
        {
            AssetGroups.Add(new GroupViewModel<AssetViewModel>(
                label,
                [.. ids.Select(id =>
                {
                    var propId = _assetMeta.GetPropId(id);
                    ulong count = propId != 0
                        ? (_activeProps.TryGetValue(propId, out var pv) ? (ulong)Math.Max(0L, pv) : 0UL)
                        : (_activeCounts.TryGetValue(id, out var c) ? c : 0UL);
                    var entry = new MaterialEntry(id, _assetMeta.GetName(id), string.Empty, count);
                    return new AssetViewModel(entry, _assetMeta);
                })]));
        }
    }

    private void RebuildFoodGroups()
    {
        FoodGroups.Clear();
        foreach (var (label, ids) in _foodMeta.Groups)
        {
            FoodGroups.Add(new GroupViewModel<FoodViewModel>(
                label,
                [.. ids
                    .Select(id => (id, meta: _foodMeta.GetMeta(id)))
                    .Where(x => x.meta is not null)
                    .Select(x =>
                    {
                        var count = _activeCounts.TryGetValue(x.id, out var c) ? c : 0UL;
                        var ingredients = x.meta!.Ingredients
                            .Select(ing =>
                            {
                                var held    = _activeCounts.TryGetValue(ing.Id, out var h) ? h : 0UL;
                                var iconUri = _materialMeta.GetMeta(ing.Id).IconUri;
                                return new IngredientViewModel(ing, held, iconUri);
                            })
                            .ToList();
                        return new FoodViewModel(x.meta!, count, ingredients);
                    })]));  
        }
    }

    private void LoadDefaultWeapons()
    {
        foreach (var e in _weaponMeta.GetDefaultEntries())
            Weapons.Add(new WeaponViewModel(e, _weaponMeta));
    }

    private void LoadDefaultArtifacts()
    {
        foreach (var e in _artifactMeta.GetDefaultEntries())
            Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
    }

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
}
