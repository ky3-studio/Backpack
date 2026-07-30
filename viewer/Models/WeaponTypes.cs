namespace Backpack.Viewer.Models;

internal static class WeaponTypes
{
    public const string Sword    = "单手剑";
    public const string Claymore = "双手剑";
    public const string Polearm  = "长柄武器";
    public const string Catalyst = "法器";
    public const string Bow      = "弓";

    public static string FromRaw(string raw) => raw switch
    {
        "sword"    => Sword,
        "claymore" => Claymore,
        "polearm"  => Polearm,
        "catalyst" => Catalyst,
        "bow"      => Bow,
        _          => string.Empty,
    };
}
