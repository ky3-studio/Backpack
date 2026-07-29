using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class FoodPage : UserControl, IDisposable
{
    public static readonly DependencyProperty FoodGroupsProperty =
        DependencyProperty.Register(nameof(FoodGroups), typeof(ObservableCollection<GroupViewModel<FoodViewModel>>), typeof(FoodPage),
            new PropertyMetadata(null, OnFoodGroupsChanged));

    public ObservableCollection<GroupViewModel<FoodViewModel>>? FoodGroups
    {
        get => (ObservableCollection<GroupViewModel<FoodViewModel>>?)GetValue(FoodGroupsProperty);
        set => SetValue(FoodGroupsProperty, value);
    }

    public IReadOnlyList<FoodViewModel>? CurrentItems =>
        (_controller.SelectedGroup as GroupViewModel<FoodViewModel>)?.Items;

    private readonly TabbedGroupController<GroupViewModel<FoodViewModel>> _controller;

    public FoodPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<FoodViewModel>>(TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        CardRepeater.ItemTemplate = new PooledElementFactory((DataTemplate)Resources["FoodCardTemplate"]);
    }

    private static void OnFoodGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FoodPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<FoodViewModel>>?)e.NewValue);
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e) =>
        _controller.OnTabSelectionChanged(e);

    private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FrameworkElement fe)
            fe.DataContext = sender.ItemsSourceView?.GetAt(args.Index);
    }

    private void OnElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is FrameworkElement fe)
            fe.DataContext = null;
    }

    private void OnCardDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is FoodViewModel vm)
        {
            UiHelper.ShowDetailFlyout(fe, vm.Name, vm.IngredientsText, maxWidth: 360);
            e.Handled = true;
        }
    }

    public void Dispose() => _controller.Dispose();
}
