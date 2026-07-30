using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace Backpack.Viewer.Services;

internal static class StaticResources
{
    public static string AssetsDir   { get; } = Path.Combine(AppContext.BaseDirectory, "Assets");
    public static string MetadataDir { get; } = Path.Combine(AppContext.BaseDirectory, "Resources");

    private const string Base = "http://8.134.75.17/static/raw/";

    public static Uri WeaponIcon(string icon)     => new($"{Base}EquipIcon/{icon}.png");
    public static Uri ArtifactIcon(string icon)   => new($"{Base}RelicIcon/{icon}.png");
    public static Uri MaterialIcon(string icon)   => new($"{Base}ItemIcon/{icon}.png");
    public static Uri AvatarIcon(string icon)     => new($"{Base}AvatarIcon/{icon}.png");
    public static Uri AvatarCard(string namecard) => new($"{Base}NameCardPic/{namecard}_P.png");
    public static Uri SkillIcon(string icon)      => new($"{Base}Skill/{icon}.png");
    public static Uri TalentIcon(string icon)     => new($"{Base}Talent/{icon}.png");
    public static Uri MonsterIcon(string icon)    => new($"{Base}MonsterIcon/{icon}.png");
    public static Uri TutorialIcon(string icon)   => new($"{Base}Tutorial/{icon}.png");

    public static Uri? DropIcon(string? icon)
    {
        if (string.IsNullOrEmpty(icon)) return null;
        var folder =
            icon.StartsWith("UI_RelicIcon", StringComparison.Ordinal) ? "RelicIcon" :
            icon.StartsWith("UI_EquipIcon", StringComparison.Ordinal) ? "EquipIcon" :
            "ItemIcon";
        return new($"{Base}{folder}/{icon}.png");
    }

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

    private static readonly BitmapImage[] _rankStarsBitmaps =
    [
        new(RankStarsIcon(1)),
        new(RankStarsIcon(2)),
        new(RankStarsIcon(3)),
        new(RankStarsIcon(4)),
        new(RankStarsIcon(5)),
    ];

    public static BitmapImage GetRankStarsBitmap(int rank) =>
        _rankStarsBitmaps[Math.Clamp(rank, 1, 5) - 1];

    private static readonly BitmapImage?[] _rollBadges = new BitmapImage?[12];

    public static BitmapImage? RollBadge(int count)
    {
        if (count is < 1 or > 11) return null;
        return _rollBadges[count] ??= new BitmapImage(new Uri($"ms-appx:///Assets/badge/badge-{count}.ico"));
    }

    public static Uri? FightPropIcon(string? prop)
    {
        if (string.IsNullOrEmpty(prop)) return null;
        var name = prop.StartsWith("FIGHT_PROP_", StringComparison.Ordinal) ? prop["FIGHT_PROP_".Length..] : prop;
        return new($"ms-appx:///Assets/UI/{name}.png");
    }

    public static Uri? ElementIcon(string? element)
    {
        if (string.IsNullOrEmpty(element)) return null;
        return new($"ms-appx:///Assets/UI/{element.ToUpperInvariant()}.png");
    }

    private static readonly Dictionary<string, string> _elementResistIcons = new()
    {
        ["Fire"]     = "PYRO",
        ["Water"]    = "HYDRO",
        ["Grass"]    = "DENDRO",
        ["Elec"]     = "ELECTRO",
        ["Wind"]     = "ANEMO",
        ["Ice"]      = "CRYO",
        ["Rock"]     = "GEO",
        ["Physical"] = "PHYSICAL_ADD_HURT",
    };

    public static BitmapImage? ElementResistBitmap(string element)
    {
        if (!_elementResistIcons.TryGetValue(element, out var name)) return null;
        return new BitmapImage(new Uri($"ms-appx:///Assets/UI/{name}.png"));
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
