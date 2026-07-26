using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed record SubStatItemViewModel(
    string     Name,
    string     ValueDisplay,
    BitmapImage BadgeSource,
    string     RollsDetail
);
