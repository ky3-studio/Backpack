using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
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
        if (sender is not FrameworkElement fe) return;
        if (fe.Tag is not FoodViewModel vm || string.IsNullOrEmpty(vm.IngredientsText)) return;

        var res   = Application.Current.Resources;
        var panel = new StackPanel { MaxWidth = 360, Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text         = vm.Name,
            Style        = res["AppBodyStrongTextBlockStyle"] as Style,
            TextWrapping = TextWrapping.WrapWholeWords
        });
        panel.Children.Add(new TextBlock
        {
            Text         = vm.IngredientsText,
            Style        = res["AppCaptionTextBlockStyle"] as Style,
            Foreground   = res["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
            TextWrapping = TextWrapping.WrapWholeWords
        });
        new Flyout { Content = panel }.ShowAt(fe);
    }

    private async void OnItemIconFailed(object sender, ExceptionRoutedEventArgs e) =>
        await GfxLoader.HandleIconFailedAsync(sender);
}
