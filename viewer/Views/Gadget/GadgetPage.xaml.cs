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

    internal IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    internal ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }
    public ICommand ApplyFilterCommand { get; }

    public IReadOnlyList<GadgetViewModel>? CurrentItems =>
        SearchTokenFilter.Apply(
            (_controller.SelectedGroup as GroupViewModel<GadgetViewModel>)?.Items,
            FilterTokens,
            ItemSearchTokens.MatchValue);

    private readonly TabbedGroupController<GroupViewModel<GadgetViewModel>> _controller;
    private PooledElementFactory _listFactory = null!;
    private PooledElementFactory _gridFactory = null!;

    public GadgetPage()
    {
        InitializeComponent();
        ApplyFilterCommand = new RelayCommand(() => Bindings.Update());
        _controller = new TabbedGroupController<GroupViewModel<GadgetViewModel>>(TabPivot, OnGroupChanged);
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
        var items = (_controller.SelectedGroup as GroupViewModel<GadgetViewModel>)?.Items ?? [];
        AvailableTokens = ItemSearchTokens.Build(items);
        FilterTokens.Clear();
        Bindings.Update();
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
