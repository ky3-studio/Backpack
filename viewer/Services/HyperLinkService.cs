using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed partial class HyperLinkService
{
    private readonly Dictionary<uint, HyperLinkEntry> _map = new();

    public void Load()
    {
        var path = Path.Combine(StaticResources.MetadataDir, "HyperLinkName", "HyperLinkName.json");
        if (!File.Exists(path)) return;
        try
        {
            var entries = JsonSerializer.Deserialize(File.ReadAllText(path), HyperLinkCtx.Default.HyperLinkEntryArray);
            if (entries is null) return;
            _map.Clear();
            foreach (var e in entries) _map[e.Id] = e;
        }
        catch { }
    }

    public bool TryGet(uint id, out string name, out string description)
    {
        if (_map.TryGetValue(id, out var e))
        {
            name        = e.Name;
            description = e.Description;
            return true;
        }
        name = description = string.Empty;
        return false;
    }

    [JsonSerializable(typeof(HyperLinkEntry[]))]
    private partial class HyperLinkCtx : JsonSerializerContext { }

    private sealed class HyperLinkEntry
    {
        [JsonPropertyName("Id")]          public uint   Id          { get; set; }
        [JsonPropertyName("Name")]        public string Name        { get; set; } = string.Empty;
        [JsonPropertyName("Description")] public string Description { get; set; } = string.Empty;
    }
}
