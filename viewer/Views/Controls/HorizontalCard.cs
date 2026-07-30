using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views.Controls;

[DependencyProperty<UIElement>("Left")]
[DependencyProperty<UIElement>("Right")]
public sealed partial class HorizontalCard : Control
{
    public HorizontalCard()
    {
        DefaultStyleKey = typeof(HorizontalCard);
    }
}
