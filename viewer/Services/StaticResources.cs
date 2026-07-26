namespace Backpack.Viewer.Services;

internal static class StaticResources
{
    private const string Base = "http://8.134.75.17/static/raw/";

    public static Uri WeaponIcon(string icon)   => new($"{Base}EquipIcon/{icon}.png");
    public static Uri ArtifactIcon(string icon) => new($"{Base}RelicIcon/{icon}.png");
    public static Uri MaterialIcon(string icon) => new($"{Base}ItemIcon/{icon}.png");

    public static Uri QualityIcon(int rank)
    {
        var name = rank switch
        {
            5 => "UI_QUALITY_ORANGE",
            4 => "UI_QUALITY_PURPLE",
            3 => "UI_QUALITY_BLUE",
            2 => "UI_QUALITY_GREEN",
            _ => "UI_QUALITY_WHITE",
        };
        return new($"ms-appx:///Assets/Quality/{name}.png");
    }
}
