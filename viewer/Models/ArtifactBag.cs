using System.Text.Json.Serialization;

namespace Backpack.Viewer.Models;

public sealed record ArtifactSubStat(
    [property: JsonPropertyName("type")]  string   Type,
    [property: JsonPropertyName("value")] double   Value,
    [property: JsonPropertyName("rolls")] double[] Rolls
);

public sealed record ArtifactEntry(
    [property: JsonPropertyName("id")]           uint              Id,
    [property: JsonPropertyName("guid")]         string            Guid,
    [property: JsonPropertyName("set")]          string            Set,
    [property: JsonPropertyName("name")]         string            Name,
    [property: JsonPropertyName("slot")]         string            Slot,
    [property: JsonPropertyName("locked")]       bool              Locked,
    [property: JsonPropertyName("level")]        int               Level,
    [property: JsonPropertyName("rank")]         int               Rank,
    [property: JsonPropertyName("initSubStats")] int               InitSubStats,
    [property: JsonPropertyName("mainStat")]     string            MainStat,
    [property: JsonPropertyName("subStats")]     ArtifactSubStat[] SubStats
);
