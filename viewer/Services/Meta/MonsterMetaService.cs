using System.IO;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed partial class MonsterMetaService
{
    private readonly Dictionary<uint, MonsterMeta> _map;
    private readonly Dictionary<int, Dictionary<int, float>> _curves;

    public MonsterMetaService()
    {
        var dir = Path.Combine(StaticResources.AssetsDir, "Monster");

        var raw = JsonLoader.Load(Path.Combine(dir, "Monster.json"), MonsterCtx.Default.RawMonsterArray) ?? [];
        _map = new Dictionary<uint, MonsterMeta>(raw.Length);
        foreach (var e in raw)
        {
            if (e.Id <= 0 || string.IsNullOrEmpty(e.Name) || string.IsNullOrEmpty(e.Icon) || e.Icon.Contains("_None"))
                continue;
            _map[(uint)e.Id] = ToMeta(e);
        }

        var curveRaw = JsonLoader.Load(Path.Combine(dir, "MonsterCurve.json"), MonsterCtx.Default.RawCurveLevelArray) ?? [];
        _curves = new Dictionary<int, Dictionary<int, float>>(curveRaw.Length);
        foreach (var lv in curveRaw)
        {
            var byType = new Dictionary<int, float>(lv.Curves.Length);
            foreach (var c in lv.Curves) byType[c.Type] = c.Value;
            _curves[lv.Level] = byType;
        }
    }

    public MonsterMeta? GetMeta(uint id) => _map.GetValueOrDefault(id);

    public IReadOnlyList<MonsterMeta> GetDefaultEntries() =>
        [.. _map.Values.OrderBy(m => TypeOrder(m.Type)).ThenBy(m => m.Id)];

    public (int Hp, int Atk, int Def) CalcStats(MonsterMeta m, int level)
    {
        if (!m.HasBaseValue) return (0, 0, 0);
        var lv = Math.Clamp(level, 1, 200);
        if (!_curves.TryGetValue(lv, out var byType)) return (0, 0, 0);
        float Mul(int curveType) => byType.TryGetValue(curveType, out var v) ? v : 1f;
        var hp  = (int)Math.Round(m.HpBase  * Mul(m.HpCurve));
        var atk = (int)Math.Round(m.AtkBase * Mul(m.AtkCurve));
        var def = (int)Math.Round(m.DefBase * Mul(m.DefCurve));
        return (hp, atk, def);
    }

    private static int TypeOrder(int type) => type switch { 1 => 0, 2 => 1, _ => 2 };

    private static MonsterMeta ToMeta(RawMonster e)
    {
        int hpCurve = 0, atkCurve = 0, defCurve = 0;
        if (e.GrowCurves is not null)
            foreach (var g in e.GrowCurves)
                switch (g.Type)
                {
                    case 1: hpCurve  = g.Value; break;
                    case 4: atkCurve = g.Value; break;
                    case 7: defCurve = g.Value; break;
                }

        var bv = e.BaseValue;
        IReadOnlyList<MonsterResist> resists = bv is null ? [] :
        [
            new("Fire",     bv.FireSubHurt),
            new("Water",    bv.WaterSubHurt),
            new("Grass",    bv.GrassSubHurt),
            new("Elec",     bv.ElecSubHurt),
            new("Wind",     bv.WindSubHurt),
            new("Ice",      bv.IceSubHurt),
            new("Rock",     bv.RockSubHurt),
            new("Physical", bv.PhysicalSubHurt),
        ];

        return new MonsterMeta(
            (uint)e.Id, e.Name ?? string.Empty, e.Title ?? string.Empty,
            e.Description ?? string.Empty, e.Icon, e.Type,
            e.Affixes ?? [], e.Drops ?? [],
            bv is not null, bv?.HpBase ?? 0f, bv?.AttackBase ?? 0f, bv?.DefenseBase ?? 0f,
            hpCurve, atkCurve, defCurve, resists);
    }

    public sealed record MonsterResist(string Element, float Value);

    public sealed record MonsterMeta(
        uint                          Id,
        string                        Name,
        string                        Title,
        string                        Description,
        string                        Icon,
        int                           Type,
        IReadOnlyList<string>         Affixes,
        IReadOnlyList<int>            Drops,
        bool                          HasBaseValue,
        float                         HpBase,
        float                         AtkBase,
        float                         DefBase,
        int                           HpCurve,
        int                           AtkCurve,
        int                           DefCurve,
        IReadOnlyList<MonsterResist>  Resists);

    [JsonSerializable(typeof(RawMonster[]))]
    [JsonSerializable(typeof(RawCurveLevel[]))]
    private partial class MonsterCtx : JsonSerializerContext { }

    private sealed class RawMonster
    {
        [JsonPropertyName("Id")]          public int             Id          { get; set; }
        [JsonPropertyName("Name")]        public string?         Name        { get; set; }
        [JsonPropertyName("Title")]       public string?         Title       { get; set; }
        [JsonPropertyName("Description")] public string?         Description { get; set; }
        [JsonPropertyName("Icon")]        public string          Icon        { get; set; } = string.Empty;
        [JsonPropertyName("Type")]        public int             Type        { get; set; }
        [JsonPropertyName("Affixes")]     public string[]?       Affixes     { get; set; }
        [JsonPropertyName("Drops")]       public int[]?          Drops       { get; set; }
        [JsonPropertyName("BaseValue")]   public RawBaseValue?   BaseValue   { get; set; }
        [JsonPropertyName("GrowCurves")]  public RawGrowCurve[]? GrowCurves  { get; set; }
    }

    private sealed class RawBaseValue
    {
        [JsonPropertyName("HpBase")]          public float HpBase          { get; set; }
        [JsonPropertyName("AttackBase")]      public float AttackBase      { get; set; }
        [JsonPropertyName("DefenseBase")]     public float DefenseBase     { get; set; }
        [JsonPropertyName("FireSubHurt")]     public float FireSubHurt     { get; set; }
        [JsonPropertyName("GrassSubHurt")]    public float GrassSubHurt    { get; set; }
        [JsonPropertyName("WaterSubHurt")]    public float WaterSubHurt    { get; set; }
        [JsonPropertyName("ElecSubHurt")]     public float ElecSubHurt     { get; set; }
        [JsonPropertyName("WindSubHurt")]     public float WindSubHurt     { get; set; }
        [JsonPropertyName("IceSubHurt")]      public float IceSubHurt      { get; set; }
        [JsonPropertyName("RockSubHurt")]     public float RockSubHurt     { get; set; }
        [JsonPropertyName("PhysicalSubHurt")] public float PhysicalSubHurt { get; set; }
    }

    private sealed class RawGrowCurve
    {
        [JsonPropertyName("Type")]  public int Type  { get; set; }
        [JsonPropertyName("Value")] public int Value { get; set; }
    }

    private sealed class RawCurveLevel
    {
        [JsonPropertyName("Level")]  public int             Level  { get; set; }
        [JsonPropertyName("Curves")] public RawCurvePoint[] Curves { get; set; } = [];
    }

    private sealed class RawCurvePoint
    {
        [JsonPropertyName("Type")]  public int   Type  { get; set; }
        [JsonPropertyName("Value")] public float Value { get; set; }
    }
}
