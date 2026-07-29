using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Backpack.Viewer.Views;

public sealed partial class WeaponPage : UserControl, IDisposable
{
    public static readonly DependencyProperty WeaponGroupsProperty =
        DependencyProperty.Register(nameof(WeaponGroups),
            typeof(ObservableCollection<GroupViewModel<WeaponViewModel>>), typeof(WeaponPage),
            new PropertyMetadata(null, OnWeaponGroupsChanged));

    public ObservableCollection<GroupViewModel<WeaponViewModel>>? WeaponGroups
    {
        get => (ObservableCollection<GroupViewModel<WeaponViewModel>>?)GetValue(WeaponGroupsProperty);
        set => SetValue(WeaponGroupsProperty, value);
    }

    private string _textQuery = string.Empty;
    private readonly ObservableCollection<FilterOption> _typeFilters = new();

    public IReadOnlyList<FilterOption>            RankFilters  { get; } =
        [.. Enumerable.Range(1, 5).Reverse().Select(FilterOption.ForRank)];
    public ObservableCollection<FilterOption>     TypeFilters  => _typeFilters;

    public IReadOnlyList<WeaponViewModel>? CurrentItems => ApplyFilters(GetBaseItems());

    public int        ActiveFilterCount          =>
        RankFilters.Count(f => f.IsSelected) + _typeFilters.Count(f => f.IsSelected);
    public Visibility ActiveFilterBadgeVisibility =>
        (RankFilters.Any(f => f.IsSelected) || _typeFilters.Any(f => f.IsSelected))
            ? Visibility.Visible : Visibility.Collapsed;

    private readonly TabbedGroupController<GroupViewModel<WeaponViewModel>> _controller;

    public WeaponPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<WeaponViewModel>>(
            TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        CardRepeater.ItemTemplate = new PooledElementFactory((DataTemplate)Resources["WeaponCardTemplate"]);
        ContentScroller.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPagePointerPressed), handledEventsToo: true);
    }

    private static void OnWeaponGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WeaponPage page)
        {
            page._controller.Bind((ObservableCollection<GroupViewModel<WeaponViewModel>>?)e.NewValue);
            page.RebuildTypeFilters((ObservableCollection<GroupViewModel<WeaponViewModel>>?)e.NewValue);
        }
    }

    private void RebuildTypeFilters(ObservableCollection<GroupViewModel<WeaponViewModel>>? groups)
    {
        _typeFilters.Clear();
        if (groups is null) return;
        foreach (var g in groups)
            _typeFilters.Add(FilterOption.ForType(g.Header));
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

    private IReadOnlyList<WeaponViewModel>? GetBaseItems()
    {
        var selectedTypes = _typeFilters.Where(f => f.IsSelected).Select(f => f.Label).ToHashSet();
        if (selectedTypes.Count > 0)
            return WeaponGroups?.SelectMany(g => g.Items)
                               .Where(vm => selectedTypes.Contains(vm.Source.Type))
                               .ToList();
        return (_controller.SelectedGroup as GroupViewModel<WeaponViewModel>)?.Items;
    }

    private IReadOnlyList<WeaponViewModel>? ApplyFilters(IReadOnlyList<WeaponViewModel>? items)
    {
        if (items is null) return null;
        var result = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(_textQuery))
        {
            var q = _textQuery.Trim();
            result = result.Where(vm => vm.Source.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        }
        var selectedRanks = RankFilters.Where(f => f.IsSelected).Select(f => f.Rank).ToHashSet();
        if (selectedRanks.Count > 0)
            result = result.Where(vm => selectedRanks.Contains(vm.Source.Rank));
        return result.ToList();
    }

    private IReadOnlyList<SearchSuggestion> GetSuggestions(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var q        = text.Trim();
        var allItems = WeaponGroups?.SelectMany(g => g.Items) ?? [];
        return allItems
            .Where(vm => vm.Source.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(8)
            .Select(vm => SearchSuggestion.ForName(vm.Source.Name))
            .ToList();
    }

    private void OnFilterToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is FilterOption option)
        {
            option.IsSelected = tb.IsChecked ?? false;
            Bindings.Update();
        }
    }

    private void OnClearFilter(object sender, RoutedEventArgs e)
    {
        foreach (var f in RankFilters)  f.IsSelected = false;
        foreach (var f in _typeFilters) f.IsSelected = false;
        Bindings.Update();
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        switch (args.Reason)
        {
            case AutoSuggestionBoxTextChangeReason.UserInput:
                sender.ItemsSource = GetSuggestions(sender.Text);
                _textQuery         = sender.Text;
                Bindings.Update();
                break;
            case AutoSuggestionBoxTextChangeReason.ProgrammaticChange when string.IsNullOrEmpty(sender.Text):
                _textQuery = string.Empty;
                Bindings.Update();
                break;
        }
    }

    private void OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is SearchSuggestion sug)
            _textQuery = sug.TextValue;
        Bindings.Update();
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        _textQuery         = args.QueryText;
        sender.ItemsSource = null;
        Bindings.Update();
    }

    private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (SearchBox.FocusState == FocusState.Unfocused) return;
        if (e.OriginalSource is DependencyObject src && IsChildOf(src, SearchBox)) return;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => FocusSink.Focus(FocusState.Pointer));
    }

    private static bool IsChildOf(DependencyObject element, DependencyObject parent)
    {
        var current = element;
        while (current is not null)
        {
            if (ReferenceEquals(current, parent)) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    public void Dispose() => _controller.Dispose();
}
