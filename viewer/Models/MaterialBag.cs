using System.Text.Json.Serialization;

namespace Backpack.Viewer.Models;

public sealed record MaterialEntry(
    [property: JsonPropertyName("id")]       uint   Id,
    [property: JsonPropertyName("name")]     string Name,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("count")]    ulong  Count
);

public sealed record MaterialBag(
    [property: JsonPropertyName("materials")] MaterialEntry[] Materials
);
