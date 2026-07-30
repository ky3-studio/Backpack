using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Services;

internal sealed class IconSink(Action<BitmapImage?> setter) : IIconUpdatable
{
    public BitmapImage? IconSource { set => setter(value); }
}
