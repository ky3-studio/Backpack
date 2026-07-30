using System.IO;

namespace Backpack.Viewer.Services;

internal readonly record struct BackpackOutputData(
    string? Weapon,
    string? Artifact,
    string? Material,
    string? Prop,
    string? Avatar)
{
    public bool HasAny =>
        Weapon is not null || Artifact is not null ||
        Material is not null || Prop is not null || Avatar is not null;
}

internal static class BackpackOutputReader
{
    public static BackpackOutputData Read(string outputDir, DateTime freshUtcThreshold) =>
        new(
            ReadFresh(outputDir, "weapon_bag.json",   freshUtcThreshold),
            ReadFresh(outputDir, "artifact_bag.json", freshUtcThreshold),
            ReadFresh(outputDir, "material_bag.json", freshUtcThreshold),
            ReadFresh(outputDir, "prop_bag.json",     freshUtcThreshold),
            ReadFresh(outputDir, "avatar_bag.json",   freshUtcThreshold));

    private static string? ReadFresh(string dir, string name, DateTime freshUtcThreshold)
    {
        try
        {
            var info = new FileInfo(Path.Combine(dir, name));
            if (!info.Exists || info.LastWriteTimeUtc < freshUtcThreshold) return null;
            var text = File.ReadAllText(info.FullName);
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }
}
