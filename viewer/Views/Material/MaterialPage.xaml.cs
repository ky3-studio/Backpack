using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class MaterialPage : UserControl, IDisposable
{
    public static readonly DependencyProperty MaterialGroupsProperty =
        DependencyProperty.Register(nameof(MaterialGroups), typeof(ObservableCollection<GroupViewModel<MaterialViewModel>>), typeof(MaterialPage),
            new PropertyMetadata(null, OnMaterialGroupsChanged));

    public ObservableCollection<GroupViewModel<MaterialViewModel>>? MaterialGroups
    {
        get => (ObservableCollection<GroupViewModel<MaterialViewModel>>?)GetValue(MaterialGroupsProperty);
        set => SetValue(MaterialGroupsProperty, value);
    }

    public IReadOnlyList<MaterialViewModel>? CurrentItems =>
        (_controller.SelectedGroup as GroupViewModel<MaterialViewModel>)?.Items;

    private readonly TabbedGroupController<GroupViewModel<MaterialViewModel>> _controller;

    public MaterialPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<MaterialViewModel>>(TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        CardRepeater.ItemTemplate = new PooledElementFactory((DataTemplate)Resources["SimpleCardTemplate"]);
    }

    private static void OnMaterialGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MaterialPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<MaterialViewModel>>?)e.NewValue);
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
