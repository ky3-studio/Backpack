using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class AssetPage : UserControl
{
    public static readonly DependencyProperty AssetGroupsProperty =
        DependencyProperty.Register(nameof(AssetGroups), typeof(ObservableCollection<AssetGroupViewModel>), typeof(AssetPage),
            new PropertyMetadata(null, OnAssetGroupsChanged));

    public ObservableCollection<AssetGroupViewModel>? AssetGroups
    {
        get => (ObservableCollection<AssetGroupViewModel>?)GetValue(AssetGroupsProperty);
        set => SetValue(AssetGroupsProperty, value);
    }

    public IReadOnlyList<AssetViewModel>? CurrentItems { get; private set; }

    public AssetPage() => InitializeComponent();

    private static void OnAssetGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AssetPage page) return;
        if (e.OldValue is ObservableCollection<AssetGroupViewModel> old)
            old.CollectionChanged -= page.OnGroupsCollectionChanged;
        if (e.NewValue is ObservableCollection<AssetGroupViewModel> groups)
        {
            groups.CollectionChanged += page.OnGroupsCollectionChanged;
            if (groups.Count > 0) { page.CurrentItems = groups[0].Items; page.Bindings.Update(); }
        }
    }

    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (AssetGroups is not { Count: > 0 }) return;
        var header = (TabPivot.SelectedItem as AssetGroupViewModel)?.Header;
        var target = (header is not null ? AssetGroups.FirstOrDefault(g => g.Header == header) : null)
                     ?? AssetGroups[0];
        CurrentItems = target.Items;
        Bindings.Update();
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabPivot.SelectedItem is AssetGroupViewModel grp)
        {
            CurrentItems = grp.Items;
            Bindings.Update();
        }
    }
}
