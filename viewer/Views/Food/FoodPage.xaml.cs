using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class FoodPage : UserControl
{
    public static readonly DependencyProperty FoodGroupsProperty =
        DependencyProperty.Register(nameof(FoodGroups), typeof(ObservableCollection<FoodGroupViewModel>), typeof(FoodPage),
            new PropertyMetadata(null, OnFoodGroupsChanged));

    public ObservableCollection<FoodGroupViewModel>? FoodGroups
    {
        get => (ObservableCollection<FoodGroupViewModel>?)GetValue(FoodGroupsProperty);
        set => SetValue(FoodGroupsProperty, value);
    }

    public IReadOnlyList<FoodViewModel>? CurrentItems { get; private set; }

    public FoodPage() => InitializeComponent();

    private static void OnFoodGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FoodPage page) return;
        if (e.OldValue is ObservableCollection<FoodGroupViewModel> old)
            old.CollectionChanged -= page.OnGroupsCollectionChanged;
        if (e.NewValue is ObservableCollection<FoodGroupViewModel> groups)
        {
            groups.CollectionChanged += page.OnGroupsCollectionChanged;
            if (groups.Count > 0) { page.CurrentItems = groups[0].Items; page.Bindings.Update(); }
        }
    }

    private void OnGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (FoodGroups is not { Count: > 0 }) return;
        var header = (TabPivot.SelectedItem as FoodGroupViewModel)?.Header;
        var target = (header is not null ? FoodGroups.FirstOrDefault(g => g.Header == header) : null)
                     ?? FoodGroups[0];
        CurrentItems = target.Items;
        Bindings.Update();
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabPivot.SelectedItem is FoodGroupViewModel grp)
        {
            CurrentItems = grp.Items;
            Bindings.Update();
        }
    }

    private void OnCardDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is FoodViewModel vm)
        {
            UiHelper.ShowDetailFlyout(fe, vm.Name, vm.IngredientsText, maxWidth: 360);
            e.Handled = true;
        }
    }
}
