using System.IO;
using System.Text.Json.Serialization;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.Services;

public sealed partial class WeaponMetaService
{
    private static readonly HashSet<string> PercentProps =
    [
        FightProps.HpPercent,
        FightProps.AttackPercent,
        FightProps.DefensePercent,
        FightProps.ChargeEfficiency,
        FightProps.CritRate,
        FightProps.CritDmg,
        FightProps.PhysicalDmg,
    ];

    private readonly Dictionary<uint, WeaponMeta>  _meta;
    private readonly Dictionary<string, float[]>   _curves;
    private readonly Dictionary<string, float[]>   _promotes;
    private readonly Dictionary<uint, ExtraMeta>   _extra;

    public WeaponMetaService()
    {
        var dir   = Path.Combine(StaticResources.AssetsDir, "Weapon");
        _meta     = JsonLoader.Load(Path.Combine(dir, "weapons.json"),        WeaponCtx.Default.WeaponMetaArray)          ?.ToDictionary(e => (uint)e.Id) ?? [];
        _curves   = JsonLoader.Load(Path.Combine(dir, "weapon_curves.json"),   WeaponCtx.Default.DictionaryStringSingleArray) ?? [];
        _promotes = JsonLoader.Load(Path.Combine(dir, "weapon_promotes.json"), WeaponCtx.Default.DictionaryStringSingleArray) ?? [];
        _extra    = JsonLoader.Load(Path.Combine(dir, "weapon_extra.json"),    WeaponCtx.Default.ExtraMetaArray)              ?.ToDictionary(e => (uint)e.Id) ?? [];
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

    public (int Atk, string Sub) CalcStats(uint id, int level, int promote)
    {
        if (!_meta.TryGetValue(id, out var m)) return (0, string.Empty);

        var l = Math.Clamp(level,   1, 90) - 1;
        var p = Math.Clamp(promote, 0, 6);

        var atkMul   = _curves.TryGetValue(m.AtkCurve, out var ac) && ac.Length > l ? ac[l] : 1f;
        var atkBonus = _promotes.TryGetValue(m.PromoteId.ToString(), out var pb) && pb.Length > p ? pb[p] : 0f;
        var atk      = (int)Math.Round(m.AtkBase * atkMul + atkBonus);

        if (string.IsNullOrEmpty(m.SubCurve) || m.SubBase == 0f)
            return (atk, string.Empty);

        var subMul = _curves.TryGetValue(m.SubCurve, out var sc) && sc.Length > l ? sc[l] : 1f;
        var rawSub = m.SubBase * subMul;
        var sub    = PercentProps.Contains(m.SubProp)
            ? $"{rawSub * 100:F1}%"
            : $"{(int)Math.Round(rawSub)}";

        return (atk, sub);
    }

    public (string Name, string Desc) GetSkill(uint id, int refine)
    {
        if (!_meta.TryGetValue(id, out var m)) return (string.Empty, string.Empty);
        var refs = m.Refinements ?? [];
        if (refs.Length == 0) return (m.PassiveName ?? string.Empty, string.Empty);
        var idx  = Math.Clamp(refine - 1, 0, refs.Length - 1);
        return (m.PassiveName ?? string.Empty, refs[idx]);
    }

    public string       GetDescription(uint id) =>
        _extra.TryGetValue(id, out var e) ? e.Description ?? string.Empty : string.Empty;

    public IReadOnlyList<uint> GetCultivationItemIds(uint id) =>
        _extra.TryGetValue(id, out var e) && e.CultivationItems is { Length: > 0 } ci
            ? ci.Select(x => (uint)x).ToArray()
            : [];

    public string GetSubProp(uint id) =>
        _meta.TryGetValue(id, out var m) ? m.SubProp : string.Empty;

    public string GetSubPropName(uint id)
    {
        if (!_meta.TryGetValue(id, out var m)) return string.Empty;
        var key = m.SubProp switch
        {
            FightProps.HpPercent        => "FightPropHpPercent",
            FightProps.AttackPercent    => "FightPropAttackPercent",
            FightProps.DefensePercent   => "FightPropDefensePercent",
            FightProps.ChargeEfficiency => "FightPropChargeEfficiency",
            FightProps.CritRate         => "FightPropCritRate",
            FightProps.CritDmg          => "FightPropCritDmg",
            FightProps.PhysicalDmg      => "FightPropPhysicalDmg",
            _                           => null,
        };
        return key is not null ? Localized.Get(key) : m.SpecialProp;
    }

    [JsonSerializable(typeof(WeaponMeta[]))]
    [JsonSerializable(typeof(Dictionary<string, float[]>))]
    [JsonSerializable(typeof(ExtraMeta[]))]
    private partial class WeaponCtx : JsonSerializerContext { }

    private sealed record WeaponMeta(
        [property: JsonPropertyName("id")]          int      Id,
        [property: JsonPropertyName("name")]        string   Name,
        [property: JsonPropertyName("rank")]        int      Rank,
        [property: JsonPropertyName("type")]        string   Type,
        [property: JsonPropertyName("specialProp")] string   SpecialProp,
        [property: JsonPropertyName("icon")]        string   Icon,
        [property: JsonPropertyName("atkBase")]     float    AtkBase,
        [property: JsonPropertyName("atkCurve")]    string   AtkCurve,
        [property: JsonPropertyName("subBase")]     float    SubBase,
        [property: JsonPropertyName("subCurve")]    string   SubCurve,
        [property: JsonPropertyName("subProp")]     string   SubProp,
        [property: JsonPropertyName("promoteId")]   int      PromoteId,
        [property: JsonPropertyName("passiveName")] string?  PassiveName,
        [property: JsonPropertyName("flavorText")]  string?  FlavorText,
        [property: JsonPropertyName("refinements")] string[]? Refinements
    );

    private sealed class ExtraMeta
    {
        [JsonPropertyName("Id")]               public int    Id               { get; set; }
        [JsonPropertyName("Description")]      public string? Description     { get; set; }
        [JsonPropertyName("CultivationItems")] public int[]? CultivationItems { get; set; }
    }
}
