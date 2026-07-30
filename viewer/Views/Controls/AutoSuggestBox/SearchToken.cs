using Backpack.Viewer.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal sealed class SearchToken
{
    private ImageSource? _icon;

    public static readonly SearchToken NotFound = new(SearchTokenKind.None, SR.ControlAutoSuggestBoxNotFoundValue, 0);

    public SearchToken(SearchTokenKind kind, string value, int order, Uri? packageIconUri = null, Uri? iconUri = null, Uri? sideIconUri = null, Color? quality = null, bool showText = true)
    {
        Value = value;
        Kind = kind;
        PackageIconUri = packageIconUri;
        IconUri = iconUri;
        SideIconUri = sideIconUri;
        Quality = quality;
        Order = order;
        ShowText = showText;
        QualityBrush = quality is { } color ? new SolidColorBrush(color) : null;
    }

    public SearchTokenKind Kind { get; }

    public string Value { get; }

    public Uri? PackageIconUri { get; }

    public Uri? IconUri { get; }

    public Uri? SideIconUri { get; }

    public Color? Quality { get; }

    public int Order { get; }

    public bool ShowText { get; }

    public Brush? QualityBrush { get; }

    public ImageSource? Icon => _icon ??= (IconUri ?? SideIconUri ?? PackageIconUri) is { } uri ? new BitmapImage(uri) : null;

    public Visibility IconVisibility => (IconUri ?? SideIconUri ?? PackageIconUri) is not null ? Visibility.Visible : Visibility.Collapsed;

    public Visibility QualityVisibility => Quality is not null ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ValueVisibility => ShowText ? Visibility.Visible : Visibility.Collapsed;

    public override string ToString()
    {
        return Value;
    }
}
