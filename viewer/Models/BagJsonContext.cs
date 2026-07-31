using System.Text.Json.Serialization;

namespace Backpack.Viewer.Models;

[JsonSerializable(typeof(WeaponEntry[]))]
[JsonSerializable(typeof(ArtifactEntry[]))]
[JsonSerializable(typeof(AvatarEntry[]))]
[JsonSerializable(typeof(MaterialEntry[]))]
[JsonSerializable(typeof(ArtifactSubStat[]))]
[JsonSerializable(typeof(SkillEntry[]))]
[JsonSerializable(typeof(PassiveEntry[]))]
[JsonSerializable(typeof(Dictionary<string, float>))]
[JsonSerializable(typeof(string[]))]
internal partial class BagJsonContext : JsonSerializerContext { }
