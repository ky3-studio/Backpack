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

    private void OnItemIconFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is not Image img) return;
        img.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
            new Uri("ms-appx:///Assets/Quality/UI_ItemIcon_None.png"));
    }
}
