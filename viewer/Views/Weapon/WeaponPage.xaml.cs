using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class WeaponPage : UserControl
{
    public static readonly DependencyProperty WeaponsProperty =
        DependencyProperty.Register(nameof(Weapons), typeof(ObservableCollection<WeaponViewModel>), typeof(WeaponPage), new PropertyMetadata(null));

    public ObservableCollection<WeaponViewModel> Weapons
    {
        get => (ObservableCollection<WeaponViewModel>)GetValue(WeaponsProperty);
        set => SetValue(WeaponsProperty, value);
    }

    public WeaponPage() => InitializeComponent();
}
