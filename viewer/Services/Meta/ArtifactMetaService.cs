using System.IO;
using System.Text.Json.Serialization;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.Services;

public sealed partial class ArtifactMetaService
{
    private readonly Dictionary<string, (int SetId, string Icon, int MaxRank)> _map;
    private readonly Dictionary<string, Dictionary<string, string>>           _pieces;
    private readonly Dictionary<string, Dictionary<string, string>>           _bonuses;
    private readonly Dictionary<int, Dictionary<string, float[]>>             _mainProps;

    public ArtifactMetaService()
    {
        var dir   = Path.Combine(StaticResources.AssetsDir, "Artifact");
        var items = JsonLoader.Load(Path.Combine(dir, "artifacts.json"), ArtifactCtx.Default.ArtifactMetaArray) ?? [];

        _map = items
            .Where(e => !string.IsNullOrEmpty(e.Name))
            .ToDictionary(
                e => e.Name,
                e => (e.Id, e.Icon, e.LevelList.Length > 0 ? e.LevelList.Max() : 4),
                StringComparer.OrdinalIgnoreCase);
        _pieces = items
            .Where(e => !string.IsNullOrEmpty(e.Name) && e.Pieces is { Count: > 0 })
            .ToDictionary(e => e.Name, e => e.Pieces!, StringComparer.OrdinalIgnoreCase);
        _bonuses = items
            .Where(e => !string.IsNullOrEmpty(e.Name) && e.Bonuses is { Count: > 0 })
            .ToDictionary(e => e.Name, e => e.Bonuses!, StringComparer.OrdinalIgnoreCase);

        var raw = JsonLoader.Load(Path.Combine(dir, "artifact_main_props.json"),
            ArtifactCtx.Default.DictionaryStringDictionaryStringSingleArray) ?? [];
        _mainProps = [];
        foreach (var (key, value) in raw)
            if (int.TryParse(key, out int rank))
                _mainProps[rank] = value;
    }

    public string GetSetBonus(string setName, int pieceCount)
    {
        if (_bonuses.TryGetValue(setName, out var byCount) &&
            byCount.TryGetValue(pieceCount.ToString(), out var desc))
            return desc;
        return string.Empty;
    }

    public IReadOnlyList<(int Count, string Desc)> GetAllSetBonuses(string setName)
    {
        if (!_bonuses.TryGetValue(setName, out var byCount)) return [];
        return [.. byCount
            .Select(kvp => (int.TryParse(kvp.Key, out var n) ? n : 0, kvp.Value))
            .Where(t => t.Item1 > 0 && !string.IsNullOrEmpty(t.Value))
            .OrderBy(t => t.Item1)];
    }

    public string GetPieceName(string setName, string slot)
    {
        if (_pieces.TryGetValue(setName, out var bySlot) && bySlot.TryGetValue(slot, out var name))
            return name;
        return slot;
    }

    private static readonly Dictionary<string, string> _shortToRaw = new()
    {
        [PropShortNames.Hp]             = FightProps.Hp,
        [PropShortNames.Attack]         = FightProps.Attack,
        [PropShortNames.Defense]        = FightProps.Defense,
        [PropShortNames.ElementMastery] = FightProps.ElementMastery,
        [PropShortNames.HpPercent]        = FightProps.HpPercent,
        [PropShortNames.AttackPercent]    = FightProps.AttackPercent,
        [PropShortNames.DefensePercent]   = FightProps.DefensePercent,
        [PropShortNames.ChargeEfficiency] = FightProps.ChargeEfficiency,
        [PropShortNames.CritRate]         = FightProps.CritRate,
        [PropShortNames.CritDmg]          = FightProps.CritDmg,
        [PropShortNames.HealBonus]        = FightProps.HealBonus,
        [PropShortNames.FireDmg]     = FightProps.FireDmg,
        [PropShortNames.ElecDmg]     = FightProps.ElecDmg,
        [PropShortNames.IceDmg]      = FightProps.IceDmg,
        [PropShortNames.WaterDmg]    = FightProps.WaterDmg,
        [PropShortNames.WindDmg]     = FightProps.WindDmg,
        [PropShortNames.RockDmg]     = FightProps.RockDmg,
        [PropShortNames.GrassDmg]    = FightProps.GrassDmg,
        [PropShortNames.PhysicalDmg] = FightProps.PhysicalDmg,
    };

    public float GetMainPropValue(int rank, int level, string mainStat)
    {
        if (!_shortToRaw.TryGetValue(mainStat, out var raw)) return 0f;
        if (!_mainProps.TryGetValue(rank, out var byProp)) return 0f;
        if (!byProp.TryGetValue(raw, out var values)) return 0f;
        return values[Math.Clamp(level, 0, values.Length - 1)];
    }

    public Uri? GetIcon(string setName, string slot)
    {
        if (!_map.TryGetValue(setName, out var meta) || string.IsNullOrEmpty(meta.Icon))
            return null;

        var lastUnderscore = meta.Icon.LastIndexOf('_');
        var baseName = lastUnderscore >= 0 ? meta.Icon[..lastUnderscore] : meta.Icon;
        return StaticResources.ArtifactIcon($"{baseName}_{SlotToIndex(slot)}");
    }

    public IReadOnlyList<ArtifactEntry> GetDefaultEntries()
    {
        string[] allSlots =
        [
            SR.SlotFlower,
            SR.SlotPlume,
            SR.SlotSands,
            SR.SlotGoblet,
            SR.SlotCirclet,
        ];
        return [.. _map
            .OrderByDescending(kvp => kvp.Value.MaxRank).ThenBy(kvp => kvp.Value.SetId)
            .SelectMany(kvp =>
                GetSlotsForIcon(kvp.Value.Icon, allSlots).Select(slot => new ArtifactEntry(
                    (uint)kvp.Value.SetId,
                    string.Empty,
                    kvp.Key,
                    GetPieceName(kvp.Key, slot),
                    slot,
                    false,
                    0,
                    kvp.Value.MaxRank,
                    0,
                    string.Empty,
                    [])))];
    }

    private static IEnumerable<string> GetSlotsForIcon(string icon, string[] allSlots)
    {
        var lastUnderscore = icon.LastIndexOf('_');
        if (lastUnderscore < 0) return allSlots;
        return icon[(lastUnderscore + 1)..] switch
        {
            "4" => allSlots,
            var s => [IconSuffixToSlot(s)],
        };
    }

    private static string IconSuffixToSlot(string suffix) => suffix switch
    {
        "1" => SR.SlotGoblet,
        "2" => SR.SlotPlume,
        "3" => SR.SlotCirclet,
        "5" => SR.SlotSands,
        _   => SR.SlotFlower,
    };

    private static int SlotToIndex(string slot)
    {
        if (slot == SR.SlotFlower)  return 4;
        if (slot == SR.SlotPlume)   return 2;
        if (slot == SR.SlotSands)   return 5;
        if (slot == SR.SlotGoblet)  return 1;
        if (slot == SR.SlotCirclet) return 3;
        return 4;
    }

    [JsonSerializable(typeof(ArtifactMeta[]))]
    [JsonSerializable(typeof(Dictionary<string, Dictionary<string, float[]>>))]
    private partial class ArtifactCtx : JsonSerializerContext { }

    private sealed record ArtifactMeta(
        [property: JsonPropertyName("id")]        int    Id,
        [property: JsonPropertyName("name")]      string Name,
        [property: JsonPropertyName("levelList")] int[]  LevelList,
        [property: JsonPropertyName("icon")]      string Icon,
        [property: JsonPropertyName("pieces")]    Dictionary<string, string>? Pieces,
        [property: JsonPropertyName("bonuses")]   Dictionary<string, string>? Bonuses
    );
}
