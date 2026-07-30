using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Views.Controls;

[DependencyProperty<BitmapImage>("Quality")]
[DependencyProperty<BitmapImage>("Icon")]
public sealed partial class ItemIcon : Control
{
    public ItemIcon()
    {
        DefaultStyleKey = typeof(ItemIcon);
    }
}
