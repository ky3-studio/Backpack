using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.Services;

public sealed class WeaponMetaService
{
    private readonly Dictionary<uint, WeaponMeta> _meta;

    public WeaponMetaService()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Assets", "weapons.json");
        try
        {
            if (File.Exists(jsonPath))
            {
                var items = JsonSerializer.Deserialize<WeaponMeta[]>(
                    File.ReadAllText(jsonPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                _meta = items.ToDictionary(e => (uint)e.Id);
                return;
            }
        }
        catch { }
        _meta = [];
    }

    public Uri? GetIcon(uint id)
    {
        if (_meta.TryGetValue(id, out var m) && !string.IsNullOrEmpty(m.Icon))
            return StaticResources.WeaponIcon(m.Icon);
        return null;
    }

    public IReadOnlyList<WeaponEntry> GetDefaultEntries() =>
        [.. _meta.Values
            .OrderByDescending(m => m.Rank).ThenBy(m => m.Id)
            .Select(m => new WeaponEntry((uint)m.Id, string.Empty, m.Name, m.Type, m.Rank, m.SpecialProp, 0, 0, 0))];

    private sealed record WeaponMeta(
        [property: JsonPropertyName("id")]          int    Id,
        [property: JsonPropertyName("name")]        string Name,
        [property: JsonPropertyName("rank")]        int    Rank,
        [property: JsonPropertyName("type")]        string Type,
        [property: JsonPropertyName("specialProp")] string SpecialProp,
        [property: JsonPropertyName("icon")]        string Icon
    );
}
