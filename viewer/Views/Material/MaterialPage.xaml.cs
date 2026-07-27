using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class MaterialPage : UserControl
{
    public static readonly DependencyProperty MaterialGroupsProperty =
        DependencyProperty.Register(nameof(MaterialGroups), typeof(ObservableCollection<MaterialGroupViewModel>), typeof(MaterialPage),
            new PropertyMetadata(null, OnMaterialGroupsChanged));

    public ObservableCollection<MaterialGroupViewModel>? MaterialGroups
    {
        get => (ObservableCollection<MaterialGroupViewModel>?)GetValue(MaterialGroupsProperty);
        set => SetValue(MaterialGroupsProperty, value);
    }

    public IReadOnlyList<MaterialViewModel>? CurrentItems { get; private set; }

    public MaterialPage() => InitializeComponent();

    private static void OnMaterialGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MaterialPage page &&
            e.NewValue is ObservableCollection<MaterialGroupViewModel> groups &&
            groups.Count > 0)
        {
            page.CurrentItems = groups[0].Items;
            page.Bindings.Update();
        }
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabPivot.SelectedItem is MaterialGroupViewModel grp)
        {
            CurrentItems = grp.Items;
            Bindings.Update();
        }
    }
}
