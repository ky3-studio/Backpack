using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class GadgetPage : UserControl, IDisposable
{
    public static readonly DependencyProperty GadgetGroupsProperty =
        DependencyProperty.Register(nameof(GadgetGroups), typeof(ObservableCollection<GroupViewModel<GadgetViewModel>>), typeof(GadgetPage),
            new PropertyMetadata(null, OnGadgetGroupsChanged));

    public ObservableCollection<GroupViewModel<GadgetViewModel>>? GadgetGroups
    {
        get => (ObservableCollection<GroupViewModel<GadgetViewModel>>?)GetValue(GadgetGroupsProperty);
        set => SetValue(GadgetGroupsProperty, value);
    }

    public IReadOnlyList<GadgetViewModel>? CurrentItems =>
        (_controller.SelectedGroup as GroupViewModel<GadgetViewModel>)?.Items;

    private readonly TabbedGroupController<GroupViewModel<GadgetViewModel>> _controller;

    public GadgetPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<GadgetViewModel>>(TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        CardRepeater.ItemTemplate = new PooledElementFactory((DataTemplate)Resources["SimpleCardTemplate"]);
    }

    private static void OnGadgetGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GadgetPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<GadgetViewModel>>?)e.NewValue);
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

    public void Dispose() => _controller.Dispose();
}
