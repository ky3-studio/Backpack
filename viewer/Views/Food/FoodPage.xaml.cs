using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class FoodPage : UserControl
{
    public static readonly DependencyProperty FoodGroupsProperty =
        DependencyProperty.Register(nameof(FoodGroups), typeof(ObservableCollection<FoodGroupViewModel>), typeof(FoodPage), new PropertyMetadata(null));

    public ObservableCollection<FoodGroupViewModel> FoodGroups
    {
        get => (ObservableCollection<FoodGroupViewModel>)GetValue(FoodGroupsProperty);
        set => SetValue(FoodGroupsProperty, value);
    }

    public FoodPage() => InitializeComponent();

    private void OnRepeaterElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FrameworkElement fe &&
            sender.ItemsSourceView?.GetAt(args.Index) is FoodViewModel vm)
        {
            fe.Tag          = vm;
            fe.DoubleTapped += OnCardDoubleTapped;
        }
    }

    private void OnRepeaterElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is FrameworkElement fe)
            fe.DoubleTapped -= OnCardDoubleTapped;
    }

    private void OnCardDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is FoodViewModel vm)
            UiHelper.ShowDetailFlyout(fe, vm.Name, vm.IngredientsText, maxWidth: 360);
    }
}
