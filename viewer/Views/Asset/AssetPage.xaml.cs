using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class AssetPage : UserControl, IDisposable
{
    public static readonly DependencyProperty AssetGroupsProperty =
        DependencyProperty.Register(nameof(AssetGroups), typeof(ObservableCollection<GroupViewModel<AssetViewModel>>), typeof(AssetPage),
            new PropertyMetadata(null, OnAssetGroupsChanged));

    public ObservableCollection<GroupViewModel<AssetViewModel>>? AssetGroups
    {
        get => (ObservableCollection<GroupViewModel<AssetViewModel>>?)GetValue(AssetGroupsProperty);
        set => SetValue(AssetGroupsProperty, value);
    }

    public IReadOnlyList<AssetViewModel>? CurrentItems =>
        (_controller.SelectedGroup as GroupViewModel<AssetViewModel>)?.Items;

    private readonly TabbedGroupController<GroupViewModel<AssetViewModel>> _controller;

    public AssetPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<AssetViewModel>>(TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        CardRepeater.ItemTemplate = new PooledElementFactory((DataTemplate)Resources["SimpleCardTemplate"]);
    }

    private static void OnAssetGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AssetPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<AssetViewModel>>?)e.NewValue);
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
