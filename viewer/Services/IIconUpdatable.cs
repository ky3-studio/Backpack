using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Services;

internal interface IIconUpdatable
{
    BitmapImage? IconSource { set; }
}
