using Microsoft.UI.Xaml.Markup;

namespace Backpack.Viewer.Localization;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
internal sealed partial class ResourceStringExtension : MarkupExtension
{
    public string Name { get; set; } = string.Empty;

    protected override object ProvideValue()
    {
        return Localized.Get(Name);
    }
}
