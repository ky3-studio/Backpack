using System.IO;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed partial class AvatarMetaService
{
    private readonly Dictionary<uint, AvatarMeta> _map;

    public AvatarMetaService()
    {
        var path = Path.Combine(StaticResources.MetadataDir, "Avatar", "avatar_meta.json");
        var raw  = JsonLoader.Load(path, AvatarCtx.Default.DictionaryStringRawEntry) ?? [];
        _map = new Dictionary<uint, AvatarMeta>(raw.Count);
        foreach (var (_, e) in raw)
            _map[(uint)e.Id] = ToMeta(e);
    }

    public AvatarMeta? GetMeta(uint id)               => _map.GetValueOrDefault(id);
    public IReadOnlyCollection<AvatarMeta> All          => _map.Values;

    public IReadOnlyList<AvatarMeta> GetDefaultEntries() =>
        [.. _map.Values
            .OrderByDescending(m => m.Rarity)
            .ThenBy(m => m.Id)];

    private static AvatarMeta ToMeta(RawEntry e) => new(
        (uint)e.Id,
        e.Name,
        e.Element,
        e.ElementCn,
        e.Rarity,
        e.WeaponType,
        e.Icon,
        e.SideIcon,
        e.Substat,
        e.SubstatProp,
        e.Birthday,
        e.Association,
        e.Namecard,
        [.. e.Skills   .Select(s => new SkillMeta   ((uint)s.Id, s.Name, s.Icon, s.Type, (uint)s.GroupId))],
        [.. e.Inherents.Select(i => new InherentMeta((uint)i.Id, i.Name, i.Icon, i.UnlockPhase, i.Description))],
        [.. e.Talents  .Select(t => new TalentMeta  (
            (uint)t.Id, t.Name, t.Icon, t.Index,
            t.ExtraLevelRaw is { } el ? new ExtraLevel(el.Index, el.Value) : null,
            t.Description))]
    );

    public sealed record ExtraLevel  (int Index, int Value);
    public sealed record SkillMeta   (uint Id, string Name, string Icon, string Type, uint GroupId);
    public sealed record InherentMeta(uint Id, string Name, string Icon, int UnlockPhase, string Description);
    public sealed record TalentMeta  (uint Id, string Name, string Icon, int Index, ExtraLevel? ExtraLevel, string Description);

    public sealed record AvatarMeta(
        uint                        Id,
        string                      Name,
        string                      Element,
        string                      ElementCn,
        int                         Rarity,
        string                      WeaponType,
        string                      Icon,
        string                      SideIcon,
        string                      Substat,
        string                      SubstatProp,
        string                      Birthday,
        int                         Association,
        string                      Namecard,
        IReadOnlyList<SkillMeta>    Skills,
        IReadOnlyList<InherentMeta> Inherents,
        IReadOnlyList<TalentMeta>   Talents
    );

    [JsonSerializable(typeof(Dictionary<string, RawEntry>))]
    private partial class AvatarCtx : JsonSerializerContext { }

    private sealed class RawEntry
    {
        [JsonPropertyName("id")]           public int           Id          { get; set; }
        [JsonPropertyName("name")]         public string        Name        { get; set; } = string.Empty;
        [JsonPropertyName("element")]      public string        Element     { get; set; } = string.Empty;
        [JsonPropertyName("element_cn")]   public string        ElementCn   { get; set; } = string.Empty;
        [JsonPropertyName("rarity")]       public int           Rarity      { get; set; }
        [JsonPropertyName("weapon_type")]  public string        WeaponType  { get; set; } = string.Empty;
        [JsonPropertyName("icon")]         public string        Icon        { get; set; } = string.Empty;
        [JsonPropertyName("side_icon")]    public string        SideIcon    { get; set; } = string.Empty;
        [JsonPropertyName("substat")]      public string        Substat     { get; set; } = string.Empty;
        [JsonPropertyName("substat_prop")] public string        SubstatProp { get; set; } = string.Empty;
        [JsonPropertyName("birthday")]     public string        Birthday    { get; set; } = string.Empty;
        [JsonPropertyName("association")]  public int           Association { get; set; }
        [JsonPropertyName("namecard")]     public string        Namecard    { get; set; } = string.Empty;
        [JsonPropertyName("skills")]       public RawSkill[]    Skills      { get; set; } = [];
        [JsonPropertyName("inherents")]    public RawInherent[] Inherents   { get; set; } = [];
        [JsonPropertyName("talents")]      public RawTalent[]   Talents     { get; set; } = [];
    }

    private sealed class RawSkill
    {
        [JsonPropertyName("id")]       public int    Id      { get; set; }
        [JsonPropertyName("name")]     public string Name    { get; set; } = string.Empty;
        [JsonPropertyName("icon")]     public string Icon    { get; set; } = string.Empty;
        [JsonPropertyName("type")]     public string Type    { get; set; } = string.Empty;
        [JsonPropertyName("group_id")] public int    GroupId { get; set; }
    }

    private sealed class RawInherent
    {
        [JsonPropertyName("id")]           public int    Id          { get; set; }
        [JsonPropertyName("name")]         public string Name        { get; set; } = string.Empty;
        [JsonPropertyName("icon")]         public string Icon        { get; set; } = string.Empty;
        [JsonPropertyName("description")]  public string Description { get; set; } = string.Empty;
        [JsonPropertyName("unlock_phase")] public int    UnlockPhase { get; set; }
    }

    private sealed class RawTalent
    {
        [JsonPropertyName("id")]          public int             Id            { get; set; }
        [JsonPropertyName("name")]        public string          Name          { get; set; } = string.Empty;
        [JsonPropertyName("icon")]        public string          Icon          { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string          Description   { get; set; } = string.Empty;
        [JsonPropertyName("index")]       public int             Index         { get; set; }
        [JsonPropertyName("extra_level")] public RawExtraLevel?  ExtraLevelRaw { get; set; }
    }

    private sealed class RawExtraLevel
    {
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("value")] public int Value { get; set; }
    }
}
