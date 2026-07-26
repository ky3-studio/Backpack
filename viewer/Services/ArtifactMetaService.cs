using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.Services;

public sealed class ArtifactMetaService
{
    private readonly Dictionary<string, (int SetId, string Icon, int MaxRank)> _map;
    private readonly Dictionary<string, Dictionary<string, string>>           _pieces;
    private readonly Dictionary<int, Dictionary<string, float[]>>             _mainProps;

    public ArtifactMetaService()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Assets", "Artifact");

        try
        {
            var jsonPath = Path.Combine(dir, "artifacts.json");
            if (File.Exists(jsonPath))
            {
                var items = JsonSerializer.Deserialize<ArtifactMeta[]>(
                    File.ReadAllText(jsonPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                _map = items
                    .Where(e => !string.IsNullOrEmpty(e.Name))
                    .ToDictionary(
                        e => e.Name,
                        e => (e.Id, e.Icon, e.LevelList.Length > 0 ? e.LevelList.Max() : 4),
                        StringComparer.OrdinalIgnoreCase);
                _pieces = items
                    .Where(e => !string.IsNullOrEmpty(e.Name) && e.Pieces is { Count: > 0 })
                    .ToDictionary(
                        e => e.Name,
                        e => e.Pieces!,
                        StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _map    = [];
                _pieces = [];
            }
        }
        catch { _map = []; _pieces = []; }

        try
        {
            var propsPath = Path.Combine(dir, "artifact_main_props.json");
            if (File.Exists(propsPath))
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, float[]>>>(
                    File.ReadAllText(propsPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                _mainProps = raw.ToDictionary(
                    kvp => int.Parse(kvp.Key),
                    kvp => kvp.Value);
            }
            else
            {
                _mainProps = [];
            }
        }
        catch { _mainProps = []; }
    }

    public string GetPieceName(string setName, string slot)
    {
        if (_pieces.TryGetValue(setName, out var bySlot) && bySlot.TryGetValue(slot, out var name))
            return name;
        return slot;
    }

    public float GetMainPropValue(int rank, int level, string propTypeRaw)
    {
        if (!_mainProps.TryGetValue(rank, out var byProp)) return 0f;
        if (!byProp.TryGetValue(propTypeRaw, out var values)) return 0f;
        var idx = Math.Clamp(level, 0, values.Length - 1);
        return values[idx];
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
            Localized.Get("SlotFlower"),
            Localized.Get("SlotPlume"),
            Localized.Get("SlotSands"),
            Localized.Get("SlotGoblet"),
            Localized.Get("SlotCirclet"),
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
                    new ArtifactMainStat(string.Empty, string.Empty),
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
        "1" => Localized.Get("SlotGoblet"),
        "2" => Localized.Get("SlotPlume"),
        "3" => Localized.Get("SlotCirclet"),
        "5" => Localized.Get("SlotSands"),
        _   => Localized.Get("SlotFlower"),
    };

    private static int SlotToIndex(string slot) => slot switch
    {
        "生之花" or "Flower of Life"     => 4,
        "死之羽" or "Plume of Death"     => 2,
        "时之沙" or "Sands of Eon"       => 5,
        "空之杯" or "Goblet of Eonothem" => 1,
        "理之冠" or "Circlet of Logos"   => 3,
        _                                    => 4,
    };

    private sealed record ArtifactMeta(
        [property: JsonPropertyName("id")]        int    Id,
        [property: JsonPropertyName("name")]      string Name,
        [property: JsonPropertyName("levelList")] int[]  LevelList,
        [property: JsonPropertyName("icon")]      string Icon,
        [property: JsonPropertyName("pieces")]    Dictionary<string, string>? Pieces
    );
}
