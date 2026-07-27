using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backpack.Viewer.Localization;

namespace Backpack.Viewer.Services;

public sealed class GadgetMetaService
{
    private static readonly (string File, string Key)[] _tabDefs =
    [
        ("gadgets_misc.json",        "GadgetTabMisc"),
        ("gadgets_consumable.json",  "GadgetTabConsumable"),
    ];

    private readonly Dictionary<uint, MetaEntry> _map = [];
    private readonly IReadOnlyList<(string Label, IReadOnlyList<uint> Ids)> _groups;

    public GadgetMetaService()
    {
        var dir    = Path.Combine(AppContext.BaseDirectory, "Assets", "Gadget");
        var groups = new List<(string Label, IReadOnlyList<uint> Ids)>(_tabDefs.Length);

        foreach (var (file, key) in _tabDefs)
        {
            var path = Path.Combine(dir, file);
            if (!File.Exists(path)) continue;
            try
            {
                var items = JsonSerializer.Deserialize<MetaEntry[]>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                var ids = new List<uint>(items.Length);
                foreach (var e in items.DistinctBy(x => x.Id).OrderByDescending(x => x.Rank).ThenBy(x => x.Id))
                {
                    _map[(uint)e.Id] = e;
                    ids.Add((uint)e.Id);
                }
                if (ids.Count > 0)
                    groups.Add((Localized.Get(key), ids));
            }
            catch { }
        }
        _groups = groups;
    }

    public (Uri? IconUri, int Rank) GetMeta(uint id)
    {
        if (_map.TryGetValue(id, out var meta) && !string.IsNullOrEmpty(meta.Icon))
            return (StaticResources.MaterialIcon(meta.Icon), meta.Rank);
        return (null, 1);
    }

    public string GetName(uint id) =>
        _map.TryGetValue(id, out var e) ? e.Name : string.Empty;

    public IReadOnlyList<(string Label, IReadOnlyList<uint> Ids)> Groups => _groups;

    private sealed record MetaEntry(
        [property: JsonPropertyName("id")]   int    Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("rank")] int    Rank,
        [property: JsonPropertyName("icon")] string Icon
    );
}