using System.Text.Json.Serialization;

namespace Backpack.Viewer.Models;

public sealed record PropBag(
    [property: JsonPropertyName("props")] IReadOnlyDictionary<uint, long> Props
);
