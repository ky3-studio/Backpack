using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed class MaterialMetaService
{
    private static readonly (string File, string Label)[] _tabDefs =
    [
        ("materials_char_ascension.json",   "\u89d2\u8272\u7a81\u7834"),
        ("materials_weapon_ascension.json", "\u6b66\u5668\u7a81\u7834"),
        ("materials_talent.json",           "\u5929\u8d4b\u6750\u6599"),
        ("materials_char_exp.json",         "\u89d2\u8272\u57f9\u517b"),
        ("materials_weapon_enhance.json",   "\u6b66\u5668\u5f3a\u5316"),
        ("materials_local_specialty.json",  "\u5730\u533a\u7279\u4ea7"),
        ("materials_ingredient.json",       "\u98df\u6750"),
    ];

    private readonly Dictionary<uint, MetaEntry> _map = [];
    private readonly IReadOnlyList<(string Label, IReadOnlyList<uint> Ids)> _groups;

    public MaterialMetaService()
    {
        var matDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Material");
        var groups = new List<(string Label, IReadOnlyList<uint> Ids)>(_tabDefs.Length);

        foreach (var (file, label) in _tabDefs)
        {
            var path = Path.Combine(matDir, file);
            if (!File.Exists(path)) continue;
            try
            {
                var items = JsonSerializer.Deserialize<MetaEntry[]>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                var ids = new List<uint>(items.Length);
                foreach (var e in items.DistinctBy(x => x.Id))
                {
                    _map[(uint)e.Id] = e;
                    ids.Add((uint)e.Id);
                }
                if (ids.Count > 0)
                    groups.Add((label, ids));
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
