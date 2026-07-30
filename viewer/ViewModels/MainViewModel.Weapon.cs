using System.Collections.ObjectModel;
using System.Text.Json;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<WeaponViewModel>                 Weapons      { get; } = [];
    public ObservableCollection<GroupViewModel<WeaponViewModel>> WeaponGroups { get; } = [];

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

    private void LoadDefaultWeapons()
    {
        foreach (var e in _weaponMeta.GetDefaultEntries())
            Weapons.Add(new WeaponViewModel(e, _weaponMeta, _materialMeta));
    }

    internal void ApplyWeapon(string json)
    {
        var entries = JsonSerializer.Deserialize(json, Default.WeaponEntryArray);
        if (entries is null) return;
        Weapons.Clear();
        foreach (var e in entries) Weapons.Add(new WeaponViewModel(e, _weaponMeta, _materialMeta));
        RebuildWeaponGroups();
        _db.SaveWeapons(entries);
    }
}
