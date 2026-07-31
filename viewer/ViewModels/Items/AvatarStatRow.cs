using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class AvatarStatRow
{
    public BitmapImage? Icon           { get; }
    public Visibility   IconVisibility { get; }
    public string       Label          { get; }
    public string       Value          { get; }
    public string       Extra          { get; }
    public Visibility   ExtraVisibility { get; }

    public AvatarStatRow(string? iconProp, string label, string value, string? extra = null)
    {
        Icon            = string.IsNullOrEmpty(iconProp) ? null : StaticResources.FightPropBitmap(iconProp);
        IconVisibility  = Icon is null ? Visibility.Collapsed : Visibility.Visible;
        Label           = label;
        Value           = value;
        Extra           = extra ?? string.Empty;
        ExtraVisibility = string.IsNullOrEmpty(extra) ? Visibility.Collapsed : Visibility.Visible;
    }
}
