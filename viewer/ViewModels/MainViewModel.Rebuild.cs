using Backpack.Viewer.Models;
using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
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
}
