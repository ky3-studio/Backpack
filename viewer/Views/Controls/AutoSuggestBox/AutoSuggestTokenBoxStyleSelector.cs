using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views.Controls.AutoSuggestBox;

internal sealed partial class AutoSuggestTokenBoxStyleSelector : StyleSelector
{
    public Style TokenStyle { get; set; } = default!;

    public Style TextStyle { get; set; } = default!;

    protected override Style SelectStyleCore(object item, DependencyObject container)
    {
        return item is ITokenStringContainer ? TextStyle : TokenStyle;
    }
}
