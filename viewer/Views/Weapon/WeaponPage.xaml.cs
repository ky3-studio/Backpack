using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class WeaponPage : UserControl, IDisposable
{
    public static readonly DependencyProperty WeaponGroupsProperty =
        DependencyProperty.Register(nameof(WeaponGroups), typeof(ObservableCollection<GroupViewModel<WeaponViewModel>>), typeof(WeaponPage),
            new PropertyMetadata(null, OnWeaponGroupsChanged));

    public ObservableCollection<GroupViewModel<WeaponViewModel>>? WeaponGroups
    {
        get => (ObservableCollection<GroupViewModel<WeaponViewModel>>?)GetValue(WeaponGroupsProperty);
        set => SetValue(WeaponGroupsProperty, value);
    }

    public IReadOnlyList<WeaponViewModel>? CurrentItems =>
        (_controller.SelectedGroup as GroupViewModel<WeaponViewModel>)?.Items;

    private readonly TabbedGroupController<GroupViewModel<WeaponViewModel>> _controller;

    public WeaponPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<WeaponViewModel>>(TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        CardRepeater.ItemTemplate = new PooledElementFactory((DataTemplate)Resources["WeaponCardTemplate"]);
    }

    private static void OnWeaponGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WeaponPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<WeaponViewModel>>?)e.NewValue);
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
