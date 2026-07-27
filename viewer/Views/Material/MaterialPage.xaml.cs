using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        if (d is not MaterialPage page) return;
        if (e.OldValue is ObservableCollection<MaterialGroupViewModel> old)
            old.CollectionChanged -= page.OnGroupsCollectionChanged;
        if (e.NewValue is ObservableCollection<MaterialGroupViewModel> groups)
        {
            groups.CollectionChanged += page.OnGroupsCollectionChanged;
            if (groups.Count > 0) { page.CurrentItems = groups[0].Items; page.Bindings.Update(); }
        }
    }

    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (MaterialGroups is not { Count: > 0 }) return;
        var header = (TabPivot.SelectedItem as MaterialGroupViewModel)?.Header;
        var target = (header is not null ? MaterialGroups.FirstOrDefault(g => g.Header == header) : null)
                     ?? MaterialGroups[0];
        CurrentItems = target.Items;
        Bindings.Update();
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
