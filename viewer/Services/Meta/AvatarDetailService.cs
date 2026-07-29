using System.Text.Json;
using System.Text.RegularExpressions;
using Backpack.Viewer.Models;

namespace Backpack.Viewer.Services;

public sealed class AvatarDetailService
{
    private readonly string                    _dir   = Path.Combine(StaticResources.AssetsDir, "AvatarDetail");
    private readonly Dictionary<uint, SnapAvatar?> _cache = new();
    private static readonly JsonSerializerOptions  _jsonOpts   = new() { PropertyNameCaseInsensitive = true };
    private static readonly Regex                  _paramRegex = new(@"\{param(\d+):([^}]+)\}", RegexOptions.Compiled);

    public (string Description, IReadOnlyList<SkillParamRow> Params) GetSkillInfo(uint avatarId, uint groupId, int level)
    {
        var avatar = Load(avatarId);
        var depot = avatar?.SkillDepot;
        var skill = depot?.Skills?.FirstOrDefault(s => s.GroupId == groupId)
                 ?? (depot?.EnergySkill is { } es && es.GroupId == groupId ? es : null);
        if (skill is null) return (string.Empty, []);

        var desc = skill.Description ?? string.Empty;

        if (level <= 0 || skill.Proud?.Descriptions is null || skill.Proud.Parameters is null)
            return (desc, []);

        var entry = skill.Proud.Parameters.FirstOrDefault(p => p.Level == level);
        if (entry?.Parameters is null) return (desc, []);

        var rows = new List<SkillParamRow>(skill.Proud.Descriptions.Count);
        foreach (var template in skill.Proud.Descriptions)
        {
            var pipe = template.IndexOf('|');
            if (pipe < 0) continue;
            var label = template[..pipe];
            var value = _paramRegex.Replace(template[(pipe + 1)..], m =>
            {
                int   idx = int.Parse(m.Groups[1].Value) - 1;
                string fmt = m.Groups[2].Value;
                if (idx < 0 || idx >= entry.Parameters.Length) return m.Value;
                float v = entry.Parameters[idx];
                return fmt switch
                {
                    "F1P" => $"{v * 100:F1}%",
                    "F2P" => $"{v * 100:F2}%",
                    "P"   => $"{v * 100:F1}%",
                    "F1"  => $"{v:F1}",
                    "F2"  => $"{v:F2}",
                    "I"   => $"{(int)Math.Round(v)}",
                    _     => $"{v}"
                };
            });
            rows.Add(new SkillParamRow(label, value));
        }
        return (desc, rows);
    }


    private SnapAvatar? Load(uint avatarId)
    {
        if (_cache.TryGetValue(avatarId, out var hit)) return hit;
        var path = Path.Combine(_dir, $"{avatarId}.json");
        if (!File.Exists(path)) { _cache[avatarId] = null; return null; }
        try
        {
            var result = JsonSerializer.Deserialize<SnapAvatar>(File.ReadAllText(path), _jsonOpts);
            _cache[avatarId] = result;
            return result;
        }
        catch { _cache[avatarId] = null; return null; }
    }


    private sealed class SnapAvatar     { public SnapSkillDepot?     SkillDepot   { get; init; } }
    private sealed class SnapSkillDepot
    {
        public List<SnapSkill>? Skills      { get; init; }
        public SnapSkill?       EnergySkill { get; init; }
    }

    private sealed class SnapSkill
    {
        public uint       GroupId     { get; init; }
        public SnapProud? Proud       { get; init; }
        public string?    Description { get; init; }
    }

    private sealed class SnapProud
    {
        public List<string>?         Descriptions { get; init; }
        public List<SnapSkillLevel>? Parameters   { get; init; }
    }

    private sealed class SnapSkillLevel
    {
        public int     Level      { get; init; }
        public float[]? Parameters { get; init; }
    }
}
