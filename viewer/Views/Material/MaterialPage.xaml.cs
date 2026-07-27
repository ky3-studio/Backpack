using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class MaterialPage : UserControl
{
    public static readonly DependencyProperty MaterialGroupsProperty =
        DependencyProperty.Register(nameof(MaterialGroups), typeof(ObservableCollection<MaterialGroupViewModel>), typeof(MaterialPage), new PropertyMetadata(null));

    public ObservableCollection<MaterialGroupViewModel> MaterialGroups
    {
        get => (ObservableCollection<MaterialGroupViewModel>)GetValue(MaterialGroupsProperty);
        set => SetValue(MaterialGroupsProperty, value);
    }

    public MaterialPage() => InitializeComponent();
}
