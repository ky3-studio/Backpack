using System.Text.Json.Serialization;

namespace Backpack.Viewer.Models;

public sealed record AvatarEntry(
    [property: JsonPropertyName("id")]      uint     Id,
    [property: JsonPropertyName("guid")]    string   Guid,
    [property: JsonPropertyName("level")]   int      Level,
    [property: JsonPropertyName("promote")] int      Promote,
    [property: JsonPropertyName("fetter")]  int      Fetter,
    [property: JsonPropertyName("talents")] uint[]   Talents,
    [property: JsonPropertyName("skills")]  int[][]  Skills,
    [property: JsonPropertyName("extras")]  int[][]  Extras,
    [property: JsonPropertyName("equips")]  string[] Equips
);

public sealed record AvatarBag(
    [property: JsonPropertyName("avatars")] AvatarEntry[] Avatars
);
