using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class GadgetPage : UserControl
{
    public static readonly DependencyProperty GadgetGroupsProperty =
        DependencyProperty.Register(nameof(GadgetGroups), typeof(ObservableCollection<GadgetGroupViewModel>), typeof(GadgetPage),
            new PropertyMetadata(null, OnGadgetGroupsChanged));

    public ObservableCollection<GadgetGroupViewModel>? GadgetGroups
    {
        get => (ObservableCollection<GadgetGroupViewModel>?)GetValue(GadgetGroupsProperty);
        set => SetValue(GadgetGroupsProperty, value);
    }

    public IReadOnlyList<GadgetViewModel>? CurrentItems { get; private set; }

    public GadgetPage() => InitializeComponent();

    private static void OnGadgetGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GadgetPage page) return;
        if (e.OldValue is ObservableCollection<GadgetGroupViewModel> old)
            old.CollectionChanged -= page.OnGroupsCollectionChanged;
        if (e.NewValue is ObservableCollection<GadgetGroupViewModel> groups)
        {
            groups.CollectionChanged += page.OnGroupsCollectionChanged;
            if (groups.Count > 0) { page.CurrentItems = groups[0].Items; page.Bindings.Update(); }
        }
    }

    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (GadgetGroups is not { Count: > 0 }) return;
        var header = (TabPivot.SelectedItem as GadgetGroupViewModel)?.Header;
        var target = (header is not null ? GadgetGroups.FirstOrDefault(g => g.Header == header) : null)
                     ?? GadgetGroups[0];
        CurrentItems = target.Items;
        Bindings.Update();
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabPivot.SelectedItem is GadgetGroupViewModel grp)
        {
            CurrentItems = grp.Items;
            Bindings.Update();
        }
    }
}
