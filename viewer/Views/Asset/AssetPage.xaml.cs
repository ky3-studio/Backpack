using System.Collections.ObjectModel;
using System.Windows.Input;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Backpack.Viewer.Views.Helpers;
using CommunityToolkit.Mvvm.Input;
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

    internal IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    internal ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }
    public ICommand ApplyFilterCommand { get; }

    public IReadOnlyList<AssetViewModel>? CurrentItems =>
        SearchTokenFilter.Apply(
            (_controller.SelectedGroup as GroupViewModel<AssetViewModel>)?.Items,
            FilterTokens,
            ItemSearchTokens.MatchValue);

    private readonly TabbedGroupController<GroupViewModel<AssetViewModel>> _controller;
    private PooledElementFactory _listFactory = null!;
    private PooledElementFactory _gridFactory = null!;

    public AssetPage()
    {
        InitializeComponent();
        ApplyFilterCommand = new RelayCommand(() => Bindings.Update());
        _controller = new TabbedGroupController<GroupViewModel<AssetViewModel>>(TabPivot, OnGroupChanged);
        SetupTemplate();
        ItemsPanelSelector.RegisterPropertyChangedCallback(LayoutSwitch.CurrentProperty, OnLayoutChanged);
    }

    private void SetupTemplate()
    {
        _listFactory = new PooledElementFactory((DataTemplate)Resources["SimpleCardTemplate"]);
        _gridFactory = new PooledElementFactory((DataTemplate)Resources["SimpleGridTemplate"]);
        CardRepeater.ItemTemplate = _listFactory;
    }

    private void OnLayoutChanged(DependencyObject sender, DependencyProperty dp)
    {
        bool grid = ItemsPanelSelector.Current == LayoutSwitch.Grid;
        CardRepeater.Layout = new UniformGridLayout
        {
            MinItemWidth     = grid ? 96 : 260,
            MinColumnSpacing = 8,
            MinRowSpacing    = 8,
            ItemsStretch     = grid ? UniformGridLayoutItemsStretch.None : UniformGridLayoutItemsStretch.Fill,
        };
        CardRepeater.ItemTemplate = grid ? _gridFactory : _listFactory;
    }

    private void OnGroupChanged()
    {
        var items = (_controller.SelectedGroup as GroupViewModel<AssetViewModel>)?.Items ?? [];
        AvailableTokens = ItemSearchTokens.Build(items);
        FilterTokens.Clear();
        Bindings.Update();
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
