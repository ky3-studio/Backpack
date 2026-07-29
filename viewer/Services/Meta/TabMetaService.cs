using System.IO;
using System.Text.Json.Serialization;
using Backpack.Viewer.Localization;

namespace Backpack.Viewer.Services;

public abstract class TabMetaService
{
    private readonly Dictionary<uint, MetaEntry> _map = [];
    private readonly IReadOnlyList<(string Label, IReadOnlyList<uint> Ids)> _groups;

    protected record MetaEntry(int Id, string Name, string Type, int Rank, string Icon, uint PropId = 0u);

    protected TabMetaService(string subDir, (string File, string Key)[] tabDefs, bool sortByRank = false)
    {
        var dir    = Path.Combine(StaticResources.AssetsDir, subDir);
        var groups = new List<(string Label, IReadOnlyList<uint> Ids)>(tabDefs.Length);

        foreach (var (file, key) in tabDefs)
        {
            var path = Path.Combine(dir, file);
            var items = JsonLoader.Load<RawEntry[]>(path) ?? [];
            if (items.Length == 0) continue;
            IEnumerable<RawEntry> seq = items.DistinctBy(x => x.Id);
            if (sortByRank)
                seq = seq.OrderByDescending(x => x.Rank).ThenBy(x => x.Id);
            var ids = new List<uint>();
            foreach (var e in seq)
            {
                _map[(uint)e.Id] = new MetaEntry(e.Id, e.Name, e.Type, e.Rank, e.Icon, e.PropId);
                ids.Add((uint)e.Id);
            }
            if (ids.Count > 0)
                groups.Add((Localized.Get(key), ids));
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

    public uint GetPropId(uint id) =>
        _map.TryGetValue(id, out var e) ? e.PropId : 0u;

    public IReadOnlyList<(string Label, IReadOnlyList<uint> Ids)> Groups => _groups;

    private sealed class RawEntry
    {
        [JsonPropertyName("id")]     public int    Id     { get; set; }
        [JsonPropertyName("name")]   public string Name   { get; set; } = string.Empty;
        [JsonPropertyName("type")]   public string Type   { get; set; } = string.Empty;
        [JsonPropertyName("rank")]   public int    Rank   { get; set; }
        [JsonPropertyName("icon")]   public string Icon   { get; set; } = string.Empty;
        [JsonPropertyName("propId")] public uint   PropId { get; set; }
    }
}
