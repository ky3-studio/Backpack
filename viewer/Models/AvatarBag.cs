using System.Text.Json.Serialization;

namespace Backpack.Viewer.Models;

public sealed record SkillEntry(
    [property: JsonPropertyName("id")]    uint Id,
    [property: JsonPropertyName("level")] int  Level
);

public sealed record PassiveEntry(
    [property: JsonPropertyName("id")]    uint Id,
    [property: JsonPropertyName("extra")] int  Extra
);

public sealed record AvatarEntry(
    [property: JsonPropertyName("id")]            uint          Id,
    [property: JsonPropertyName("name")]          string?       Name,
    [property: JsonPropertyName("element")]       string?       Element,
    [property: JsonPropertyName("rarity")]        int           Rarity,
    [property: JsonPropertyName("level")]         int           Level,
    [property: JsonPropertyName("ascension")]     int           Ascension,
    [property: JsonPropertyName("friendship")]    int           Friendship,
    [property: JsonPropertyName("constellation")] int           Constellation,
    [property: JsonPropertyName("skills")]        SkillEntry[]  Skills,
    [property: JsonPropertyName("passives")]      PassiveEntry[] Passives,
    [property: JsonPropertyName("equips")]        string[]      Equips,
    [property: JsonPropertyName("fightProps")]    Dictionary<string, float>? FightProps = null
);
