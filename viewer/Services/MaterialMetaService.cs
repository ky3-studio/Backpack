using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.Services;

public sealed class MaterialMetaService
{
    private readonly Dictionary<uint, MetaEntry> _map;

    public MaterialMetaService()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Material", "materials.json");
        try
        {
            if (File.Exists(jsonPath))
            {
                var items = JsonSerializer.Deserialize<MetaEntry[]>(
                    File.ReadAllText(jsonPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                _map = items.ToDictionary(e => (uint)e.Id);
                return;
            }
        }
        catch { }
        _map = [];
    }

    public (Uri? IconUri, int Rank) GetMeta(uint id)
    {
        if (_map.TryGetValue(id, out var meta) && !string.IsNullOrEmpty(meta.Icon))
            return (StaticResources.MaterialIcon(meta.Icon), meta.Rank);
        return (null, 1);
    }

    public static string TypeLabel(string type) => type switch
    {
        "specialCurrency" or "commonCurrency"                         => "货币",
        "limitedWishingItem" or "superiorVoucher"                     => "纠缠之缘",
        "wishingItem"        or "commonVoucher"                       => "相遇之缘",
        "challengeResultItem"                                         => "挑战奖励",
        "systemAccess"       or "increasesFriendship"                 => "通用道具",
        "cityStatesSigil"                                             => "城邦印记",
        "characterAscensionMaterial"                                  => "角色突破",
        "weaponAscensionMaterial"                                     => "武器突破",
        "characterTalentMaterial"                                     => "天赋材料",
        "characterEXPMaterial" or "characterLevelUpMaterial"
            or "characterandWeaponEnhancementMaterial"                => "角色培养",
        "weaponEnhancementMaterial"                                   => "武器强化",
        string s when s.StartsWith("localSpecialty")                  => "地区特产",
        "cookingIngredient" or "consumable"                           => "食材",
        "forgingOre"                                                  => "锻造矿石",
        "adventureItem" or "material"                                 => "通用材料",
        _                                                             => "其他",
    };

    private static readonly string[] _groupOrder =
    [
        "货币", "纠缠之缘", "相遇之缘", "挑战奖励", "通用道具",
        "城邦印记", "角色突破", "武器突破", "天赋材料", "角色培养",
        "武器强化", "地区特产", "食材", "锻造矿石", "通用材料", "其他"
    ];

    public static int LabelOrder(string label)
    {
        var idx = Array.IndexOf(_groupOrder, label);
        return idx >= 0 ? idx : 99;
    }

    public IReadOnlyList<MaterialEntry> GetDefaultEntries() =>
        [.. _map.Values
            .OrderBy(m => m.Id)
            .Select(m => new MaterialEntry((uint)m.Id, m.Name, m.Type, 0))];

    private sealed record MetaEntry(
        [property: JsonPropertyName("id")]   int    Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("rank")] int    Rank,
        [property: JsonPropertyName("icon")] string Icon
    );
}
