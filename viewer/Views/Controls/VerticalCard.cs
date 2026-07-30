using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views.Controls;

[DependencyProperty<UIElement>("Top")]
[DependencyProperty<UIElement>("Bottom")]
public sealed partial class VerticalCard : Control
{
    public VerticalCard()
    {
        DefaultStyleKey = typeof(VerticalCard);
    }
}
