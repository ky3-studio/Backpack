using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;

namespace Backpack.Viewer.ViewModels.Avatar;

internal static class AvatarSearchTokens
{
    public static IReadOnlyDictionary<string, SearchToken> Build(IEnumerable<AvatarViewModel> avatars) =>
        new SearchTokenBuilder<AvatarViewModel>(avatars)
            .PerItem(SearchTokenKind.Avatar, vm => vm.Name, vm => vm.IconUri)
            .GroupedMonoIcon(SearchTokenKind.ElementName, vm => vm.Element, vm => vm.ElementIconUri)
            .WeaponTypes(vm => vm.WeaponTypeName)
            .Quality(vm => vm.Rarity)
            .Build();
}