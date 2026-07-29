using System.Collections.ObjectModel;
using System.Text.Json;
using Backpack.Viewer.Models;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<GroupViewModel<MaterialViewModel>>  MaterialGroups { get; } = [];
    public ObservableCollection<GroupViewModel<FoodViewModel>>      FoodGroups     { get; } = [];
    public ObservableCollection<GroupViewModel<GadgetViewModel>>    GadgetGroups   { get; } = [];
    public ObservableCollection<GroupViewModel<AssetViewModel>>     AssetGroups    { get; } = [];

    private readonly Dictionary<uint, ulong> _activeCounts;
    private readonly Dictionary<uint, long>  _activeProps;

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

    internal void ApplyMaterial(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.MaterialEntryArray);
        if (entries is null) return;
        foreach (var e in entries)
            _activeCounts[e.Id] = e.Count;
        _db.SaveMaterials(_activeCounts);
        RebuildMaterialGroups();
        RebuildFoodGroups();
        RebuildGadgetGroups();
        RebuildAssetGroups();
    }

    internal void ApplyProp(string json)
    {
        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            uint id = PropKeyToId(prop.Name);
            if (id != 0) _activeProps[id] = prop.Value.GetInt64();
        }
        _db.SaveProps(_activeProps);
        RebuildAssetGroups();
    }
}
