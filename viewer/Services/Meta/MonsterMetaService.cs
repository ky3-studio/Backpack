using System.IO;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed partial class MonsterMetaService
{
    public static readonly IReadOnlyList<double> CoopHpMultipliers = [1.0, 1.5, 2.0, 2.5];

    private readonly List<MonsterMeta> _monsters = [];
    private readonly Dictionary<int, Dictionary<string, float>> _curves = [];

    public MonsterMetaService()
    {
        var dir = Path.Combine(StaticResources.MetadataDir, "Monster");

        var curveDoc = JsonLoader.Load(Path.Combine(dir, "MonsterCurve", "MonsterCurve.json"), MonsterCtx.Default.RawCurveResponse);
        if (curveDoc?.Data is not null)
            foreach (var (levelKey, entry) in curveDoc.Data)
                if (int.TryParse(levelKey, out var level) && entry.CurveInfos is not null)
                    _curves[level] = entry.CurveInfos;

        if (Directory.Exists(dir))
            foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
            {
                var doc = JsonLoader.Load(file, MonsterCtx.Default.RawMonsterResponse);
                if (doc?.Data is { } d && d.Id > 0 && !string.IsNullOrEmpty(d.Name) &&
                    !string.IsNullOrEmpty(d.Icon) && !d.Icon.Contains("_None"))
                    _monsters.Add(ToMeta(d));
            }
    }

    public IReadOnlyList<MonsterMeta> GetDefaultEntries() =>
        [.. _monsters.OrderBy(m => m.Type).ThenBy(m => m.Id)];

    public (int Hp, int Atk, int Def) CalcStats(MonsterVariant v, int level)
    {
        if (!v.HasBaseValue) return (0, 0, 0);
        var lv = Math.Clamp(level, 1, 200);
        if (!_curves.TryGetValue(lv, out var byKey)) return (0, 0, 0);
        float Mul(string key) => !string.IsNullOrEmpty(key) && byKey.TryGetValue(key, out var m) ? m : 1f;
        var hp  = (int)Math.Round(v.HpBase  * Mul(v.HpCurve));
        var atk = (int)Math.Round(v.AtkBase * Mul(v.AtkCurve));
        var def = (int)Math.Round(v.DefBase * Mul(v.DefCurve));
        return (hp, atk, def);
    }

    private static MonsterMeta ToMeta(RawData d)
    {
        MonsterVariant? variant = null;
        if (d.Entries is not null)
        {
            RawVariant? chosen = null;
            foreach (var raw in d.Entries.Values)
            {
                chosen ??= raw;
                if (raw.Id == d.Id) { chosen = raw; break; }
            }
            if (chosen is not null)
                variant = ToVariant(chosen);
        }

        List<MonsterTip> tips = [];
        if (d.Tips is not null)
            foreach (var (_, tip) in d.Tips)
                if (!string.IsNullOrEmpty(tip.Description) || tip.Images is { Length: > 0 })
                    tips.Add(new MonsterTip(tip.Images ?? [], tip.Description ?? string.Empty));

        return new MonsterMeta(
            (uint)d.Id, d.Name ?? string.Empty, d.Title ?? string.Empty,
            d.SpecialName ?? string.Empty, d.Type ?? string.Empty,
            d.Icon, d.Description ?? string.Empty, variant, tips);
    }

    private static MonsterVariant ToVariant(RawVariant raw)
    {
        string hpCurve = string.Empty, atkCurve = string.Empty, defCurve = string.Empty;
        float hpBase = 0f, atkBase = 0f, defBase = 0f;
        var hasBase = raw.Prop is { Length: > 0 };
        if (raw.Prop is not null)
            foreach (var p in raw.Prop)
                switch (p.PropType)
                {
                    case "FIGHT_PROP_BASE_HP":      hpBase  = p.InitValue; hpCurve  = p.Type ?? string.Empty; break;
                    case "FIGHT_PROP_BASE_ATTACK":  atkBase = p.InitValue; atkCurve = p.Type ?? string.Empty; break;
                    case "FIGHT_PROP_BASE_DEFENSE": defBase = p.InitValue; defCurve = p.Type ?? string.Empty; break;
                }

        var r = raw.Resistance;
        IReadOnlyList<MonsterResist> resists = r is null ? [] :
        [
            new("Fire",     r.FireSubHurt),
            new("Water",    r.WaterSubHurt),
            new("Grass",    r.GrassSubHurt),
            new("Elec",     r.ElecSubHurt),
            new("Wind",     r.WindSubHurt),
            new("Ice",      r.IceSubHurt),
            new("Rock",     r.RockSubHurt),
            new("Physical", r.PhysicalSubHurt),
        ];

        List<MonsterDrop> drops = [];
        if (raw.Reward is not null)
            foreach (var (idKey, rw) in raw.Reward)
                if (uint.TryParse(idKey, out var did) && !string.IsNullOrEmpty(rw.Name))
                    drops.Add(new MonsterDrop(did, rw.Name, rw.Rank, rw.Icon ?? string.Empty, rw.Count));

        return new MonsterVariant(
            (uint)raw.Id, raw.Type ?? string.Empty, raw.Type == "MONSTER_BOSS",
            hasBase, hpBase, atkBase, defBase,
            hpCurve, atkCurve, defCurve, resists, drops);
    }

    public sealed record MonsterMeta(
        uint                      Id,
        string                    Name,
        string                    Title,
        string                    SpecialName,
        string                    Type,
        string                    Icon,
        string                    Description,
        MonsterVariant?           Variant,
        IReadOnlyList<MonsterTip> Tips);

    public sealed record MonsterVariant(
        uint                         Id,
        string                       VariantType,
        bool                         IsBoss,
        bool                         HasBaseValue,
        float                        HpBase,
        float                        AtkBase,
        float                        DefBase,
        string                       HpCurve,
        string                       AtkCurve,
        string                       DefCurve,
        IReadOnlyList<MonsterResist> Resists,
        IReadOnlyList<MonsterDrop>   Drops);

    public sealed record MonsterResist(string Element, float Value);

    public sealed record MonsterDrop(uint Id, string Name, int Rank, string Icon, string? Count);

    public sealed record MonsterTip(IReadOnlyList<string> Images, string Description);

    [JsonSerializable(typeof(RawMonsterResponse))]
    [JsonSerializable(typeof(RawCurveResponse))]
    private partial class MonsterCtx : JsonSerializerContext { }

    private sealed class RawMonsterResponse
    {
        [JsonPropertyName("data")] public RawData? Data { get; set; }
    }

    private sealed class RawData
    {
        [JsonPropertyName("id")]          public int                             Id          { get; set; }
        [JsonPropertyName("name")]        public string?                         Name        { get; set; }
        [JsonPropertyName("type")]        public string?                         Type        { get; set; }
        [JsonPropertyName("icon")]        public string                          Icon        { get; set; } = string.Empty;
        [JsonPropertyName("title")]       public string?                         Title       { get; set; }
        [JsonPropertyName("specialName")] public string?                         SpecialName { get; set; }
        [JsonPropertyName("description")] public string?                         Description { get; set; }
        [JsonPropertyName("entries")]     public Dictionary<string, RawVariant>? Entries     { get; set; }
        [JsonPropertyName("tips")]        public Dictionary<string, RawTip>?     Tips        { get; set; }
    }

    private sealed class RawVariant
    {
        [JsonPropertyName("id")]         public int                            Id         { get; set; }
        [JsonPropertyName("type")]       public string?                        Type       { get; set; }
        [JsonPropertyName("prop")]       public RawProp[]?                     Prop       { get; set; }
        [JsonPropertyName("resistance")] public RawResistance?                 Resistance { get; set; }
        [JsonPropertyName("reward")]     public Dictionary<string, RawReward>? Reward     { get; set; }
    }

    private sealed class RawProp
    {
        [JsonPropertyName("propType")]  public string? PropType  { get; set; }
        [JsonPropertyName("initValue")] public float   InitValue { get; set; }
        [JsonPropertyName("type")]      public string? Type      { get; set; }
    }

    private sealed class RawResistance
    {
        [JsonPropertyName("fireSubHurt")]     public float FireSubHurt     { get; set; }
        [JsonPropertyName("grassSubHurt")]    public float GrassSubHurt    { get; set; }
        [JsonPropertyName("waterSubHurt")]    public float WaterSubHurt    { get; set; }
        [JsonPropertyName("elecSubHurt")]     public float ElecSubHurt     { get; set; }
        [JsonPropertyName("windSubHurt")]     public float WindSubHurt     { get; set; }
        [JsonPropertyName("iceSubHurt")]      public float IceSubHurt      { get; set; }
        [JsonPropertyName("rockSubHurt")]     public float RockSubHurt     { get; set; }
        [JsonPropertyName("physicalSubHurt")] public float PhysicalSubHurt { get; set; }
    }

    private sealed class RawReward
    {
        [JsonPropertyName("name")]  public string? Name  { get; set; }
        [JsonPropertyName("rank")]  public int     Rank  { get; set; }
        [JsonPropertyName("icon")]  public string? Icon  { get; set; }
        [JsonPropertyName("count")] public string? Count { get; set; }
    }

    private sealed class RawTip
    {
        [JsonPropertyName("images")]      public string[]? Images      { get; set; }
        [JsonPropertyName("description")] public string?   Description { get; set; }
    }

    private sealed class RawCurveResponse
    {
        [JsonPropertyName("data")] public Dictionary<string, RawCurveLevel>? Data { get; set; }
    }

    private sealed class RawCurveLevel
    {
        [JsonPropertyName("curveInfos")] public Dictionary<string, float>? CurveInfos { get; set; }
    }
}
