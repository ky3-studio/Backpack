using System.Collections.ObjectModel;
using System.Windows.Input;
using Backpack.Viewer.Services;
using Backpack.Viewer.Services.Story;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Views;

public sealed partial class WeaponPage : UserControl
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

    private readonly WeaponStoryService _storyService = new();
    private readonly WeaponGuideService _guideService = new();

    private WeaponViewModel? _selectedWeapon;
    private IReadOnlyList<WeaponViewModel>? _currentItems;

    internal IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    internal ObservableCollection<SearchToken>         FilterTokens { get; } = [];
    public string?                                   FilterText   { get; set; }
    public ICommand                                  FilterCommand { get; }

    public IReadOnlyList<WeaponViewModel>? CurrentItems   => _currentItems;
    public WeaponViewModel?                SelectedWeapon => _selectedWeapon;
    public string?                         WeaponStory    { get; private set; }
    public Visibility WeaponStoryVisibility => string.IsNullOrEmpty(WeaponStory) ? Visibility.Collapsed : Visibility.Visible;
    public IReadOnlyList<WeaponRecommendAvatar> WeaponGuideBuilds { get; private set; } = [];
    public IReadOnlyList<WeaponRecommendAvatar> WeaponGuideAbyss  { get; private set; } = [];
    public Visibility WeaponGuideVisibility => (WeaponGuideBuilds.Count > 0 || WeaponGuideAbyss.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmptyStateVisibility    => _selectedWeapon is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedWeaponVisibility => _selectedWeapon is null ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HasItemsVisibility       => (CurrentItems?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

    public string RefineDesc0 => _selectedWeapon?.RefinementDescriptions.Count > 0 ? _selectedWeapon.RefinementDescriptions[0] : string.Empty;
    public string RefineDesc1 => _selectedWeapon?.RefinementDescriptions.Count > 1 ? _selectedWeapon.RefinementDescriptions[1] : string.Empty;
    public string RefineDesc2 => _selectedWeapon?.RefinementDescriptions.Count > 2 ? _selectedWeapon.RefinementDescriptions[2] : string.Empty;
    public string RefineDesc3 => _selectedWeapon?.RefinementDescriptions.Count > 3 ? _selectedWeapon.RefinementDescriptions[3] : string.Empty;
    public string RefineDesc4 => _selectedWeapon?.RefinementDescriptions.Count > 4 ? _selectedWeapon.RefinementDescriptions[4] : string.Empty;

    public WeaponPage()
    {
        FilterCommand = new RelayCommand(OnFilterChanged);
        InitializeComponent();
        ContentScroller.AddHandler(PointerPressedEvent,
            new PointerEventHandler(OnPagePointerPressed), handledEventsToo: true);
    }

    private static void OnWeaponGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WeaponPage page)
            page.RebuildTokens((ObservableCollection<GroupViewModel<WeaponViewModel>>?)e.NewValue);
    }

    private void RebuildTokens(ObservableCollection<GroupViewModel<WeaponViewModel>>? groups)
    {
        IEnumerable<WeaponViewModel> items = groups?.SelectMany(g => g.Items) ?? [];
        AvailableTokens = WeaponSearchTokens.Build(items);
        FilterTokens.Clear();
        RefreshItems();

        if (_selectedWeapon is null && CurrentItems?.Count > 0)
            SelectWeapon(CurrentItems[0]);

        Bindings.Update();
    }

    private void OnFilterChanged()
    {
        RefreshItems();
        Bindings.Update();
    }

    private void RefreshItems()
    {
        _currentItems = ApplyFilters(GetBaseItems());
    }

    private IReadOnlyList<WeaponViewModel>? GetBaseItems() =>
        WeaponGroups?.SelectMany(g => g.Items).ToList();

    private IReadOnlyList<WeaponViewModel>? ApplyFilters(IReadOnlyList<WeaponViewModel>? items)
    {
        if (items is null) return null;
        if (FilterTokens.Count == 0) return items;

        var result = items.AsEnumerable();
        foreach (var group in FilterTokens.GroupBy(t => t.Kind))
        {
            var kind   = group.Key;
            var values = group.Select(t => t.Value).ToHashSet();
            result = result.Where(vm => values.Contains(MatchValue(vm, kind)));
        }
        return result.ToList();
    }

    private static string MatchValue(WeaponViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.Weapon        => vm.Source.Name,
        SearchTokenKind.WeaponType    => vm.Source.Type,
        SearchTokenKind.ItemQuality   => WeaponSearchTokens.RankLabel(vm.Source.Rank),
        SearchTokenKind.FightProperty => vm.SubPropName,
        _                             => string.Empty,
    };

    private void OnWeaponSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = e.AddedItems.OfType<WeaponViewModel>().FirstOrDefault();
        if (vm is null && _selectedWeapon is not null) return;
        SelectWeapon(vm);
        Bindings.Update();
    }

    private void SelectWeapon(WeaponViewModel? vm)
    {
        _selectedWeapon = vm;
        WeaponStory = null;
        WeaponGuideBuilds = [];
        WeaponGuideAbyss = [];
        if (vm is not null)
        {
            LevelSlider.SetWeapon(vm.Source.Id);
            _ = FetchWeaponStoryAsync(vm.Source.Id);
            _ = FetchWeaponGuidesAsync(vm.Source.Id);
        }
    }

    private async Task FetchWeaponStoryAsync(uint weaponId)
    {
        var story = await _storyService.FetchStoryAsync(weaponId).ConfigureAwait(false);
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_selectedWeapon?.Source.Id != weaponId) return;
            WeaponStory = story;
            Bindings.Update();
        });
    }

    private async Task FetchWeaponGuidesAsync(uint weaponId)
    {
        var guides = await _guideService.FetchGuidesAsync(weaponId).ConfigureAwait(false);
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_selectedWeapon?.Source.Id != weaponId) return;
            WeaponGuideBuilds = [.. (guides?.Builds ?? []).Select(ToRecommendAvatar)];
            WeaponGuideAbyss  = [.. (guides?.Abyss ?? []).Select(ToRecommendAvatar)];
            Bindings.Update();
        });
    }

    private static WeaponRecommendAvatar ToRecommendAvatar(WeaponGuideService.AvatarGuide g) =>
        new(new BitmapImage(StaticResources.AvatarIcon(g.Icon)), StaticResources.GetQualityBitmap(g.Rank), g.Name);

    private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (SearchBox.FocusState == FocusState.Unfocused) return;
        if (e.OriginalSource is DependencyObject src && IsChildOf(src, SearchBox)) return;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => _ = FocusManager.TryFocusAsync(ContentScroller, FocusState.Pointer));
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
}
