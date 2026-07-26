namespace Backpack.Viewer.Services;

internal static class StaticResources
{
    private const string Base = "http://8.134.75.17/static/raw/";

    public static Uri WeaponIcon(string icon)   => new($"{Base}EquipIcon/{icon}.png");
    public static Uri ArtifactIcon(string icon) => new($"{Base}RelicIcon/{icon}.png");
    public static Uri MaterialIcon(string icon) => new($"{Base}ItemIcon/{icon}.png");
}
