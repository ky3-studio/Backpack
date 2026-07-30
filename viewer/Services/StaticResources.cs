using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace Backpack.Viewer.Services;

internal static class StaticResources
{
    public static string AssetsDir { get; } = Path.Combine(AppContext.BaseDirectory, "Assets");

    private const string Base = "http://8.134.75.17/static/raw/";

    public static Uri WeaponIcon(string icon)     => new($"{Base}EquipIcon/{icon}.png");
    public static Uri ArtifactIcon(string icon)   => new($"{Base}RelicIcon/{icon}.png");
    public static Uri MaterialIcon(string icon)   => new($"{Base}ItemIcon/{icon}.png");
    public static Uri AvatarIcon(string icon)     => new($"{Base}AvatarIcon/{icon}.png");
    public static Uri AvatarCard(string namecard) => new($"{Base}NameCardPic/{namecard}_P.png");
    public static Uri SkillIcon(string icon)      => new($"{Base}Skill/{icon}.png");
    public static Uri TalentIcon(string icon)     => new($"{Base}Talent/{icon}.png");

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

    public static Uri RankStarsIcon(int rank)
    {
        var name = Math.Clamp(rank, 1, 5) switch
        {
            5 => "FIVE_STAR",
            4 => "FOUR_STAR",
            3 => "THREE_STAR",
            2 => "TWO_STAR",
            _ => "ONE_STAR",
        };
        return new($"ms-appx:///Assets/UI/{name}.png");
    }

    private static readonly BitmapImage[] _qualityBitmaps =
    [
        new(QualityIcon(0)),
        new(QualityIcon(1)),
        new(QualityIcon(2)),
        new(QualityIcon(3)),
        new(QualityIcon(4)),
        new(QualityIcon(5)),
    ];

    public static BitmapImage GetQualityBitmap(int rank) =>
        _qualityBitmaps[Math.Clamp(rank, 0, 5)];

    public static Uri? FightPropIcon(string? prop)
    {
        if (string.IsNullOrEmpty(prop)) return null;
        var name = prop.StartsWith("FIGHT_PROP_", StringComparison.Ordinal) ? prop["FIGHT_PROP_".Length..] : prop;
        return new($"ms-appx:///Assets/UI/{name}.png");
    }

    private static readonly Dictionary<string, BitmapImage> _fightPropBitmaps = [];

    public static BitmapImage? FightPropBitmap(string? prop)
    {
        if (FightPropIcon(prop) is not { } uri) return null;
        if (!_fightPropBitmaps.TryGetValue(prop!, out var bmp))
        {
            bmp = new BitmapImage(uri);
            _fightPropBitmaps[prop!] = bmp;
        }
        return bmp;
    }
}
