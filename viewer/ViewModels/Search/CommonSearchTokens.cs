using Backpack.Viewer.Services;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Search;

internal static class CommonSearchTokens
{
    public static string RankLabel(int rank)
    {
        return Math.Clamp(rank, 1, 5).ToString();
    }

    public static SearchToken Quality(int rank, int order) =>
        new(SearchTokenKind.ItemQuality, RankLabel(rank), order, iconUri: StaticResources.RankStarsIcon(rank), showText: false);

    public static SearchToken WeaponType(string type, int order) =>
        new(SearchTokenKind.WeaponType, type, order, iconUri: WeaponTypeIconUri(type));

    public static Uri? WeaponTypeIconUri(string type) =>
        WeaponViewModel.TypeIconName(type) is { } icon ? StaticResources.SkillIcon(icon) : null;
}
