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

    internal IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    internal ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }
    public ICommand ApplyFilterCommand { get; }

    public IReadOnlyList<FoodViewModel>? CurrentItems =>
        SearchTokenFilter.Apply(
            (_controller.SelectedGroup as GroupViewModel<FoodViewModel>)?.Items,
            FilterTokens,
            ItemSearchTokens.MatchValue);

    private readonly TabbedGroupController<GroupViewModel<FoodViewModel>> _controller;
    private PooledElementFactory _listFactory = null!;
    private PooledElementFactory _gridFactory = null!;

    public FoodPage()
    {
        InitializeComponent();
        ApplyFilterCommand = new RelayCommand(() => Bindings.Update());
        _controller = new TabbedGroupController<GroupViewModel<FoodViewModel>>(TabPivot, OnGroupChanged);
        SetupTemplate();
        ItemsPanelSelector.RegisterPropertyChangedCallback(LayoutSwitch.CurrentProperty, OnLayoutChanged);
    }

    private void SetupTemplate()
    {
        _listFactory = new PooledElementFactory((DataTemplate)Resources["FoodCardTemplate"]);
        _gridFactory = new PooledElementFactory((DataTemplate)Resources["FoodGridTemplate"]);
        CardRepeater.ItemTemplate = _listFactory;
    }

    private void OnLayoutChanged(DependencyObject sender, DependencyProperty dp)
    {
        bool grid = ItemsPanelSelector.Current == LayoutSwitch.Grid;
        CardRepeater.Layout = new UniformGridLayout
        {
            MinItemWidth     = grid ? 96 : 360,
            MinColumnSpacing = 8,
            MinRowSpacing    = 8,
            ItemsStretch     = grid ? UniformGridLayoutItemsStretch.None : UniformGridLayoutItemsStretch.Fill,
        };
        CardRepeater.ItemTemplate = grid ? _gridFactory : _listFactory;
    }

    private void OnGroupChanged()
    {
        var items = (_controller.SelectedGroup as GroupViewModel<FoodViewModel>)?.Items ?? [];
        AvailableTokens = ItemSearchTokens.Build(items);
        FilterTokens.Clear();
        Bindings.Update();
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
