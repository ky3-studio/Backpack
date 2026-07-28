using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed class HyperLinkService
{
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };
    private readonly Dictionary<uint, HyperLinkEntry> _map = new();

    public void Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "HyperLinkName.json");
        if (!File.Exists(path)) return;
        try
        {
            var entries = JsonSerializer.Deserialize<HyperLinkEntry[]>(File.ReadAllText(path), _opts);
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

    private sealed class HyperLinkEntry
    {
        [JsonPropertyName("Id")]          public uint   Id          { get; set; }
        [JsonPropertyName("Name")]        public string Name        { get; set; } = string.Empty;
        [JsonPropertyName("Description")] public string Description { get; set; } = string.Empty;
    }
}
