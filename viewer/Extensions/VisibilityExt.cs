using Microsoft.UI.Xaml;

namespace Backpack.Viewer;

internal static class VisibilityExt
{
    internal static Visibility ToVisibility(this bool value) =>
        value ? Visibility.Visible : Visibility.Collapsed;

    internal static Visibility ToCollapsed(this bool value) =>
        value ? Visibility.Collapsed : Visibility.Visible;
}
