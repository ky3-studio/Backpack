using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.Services.Story;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Weapon;

internal sealed class WeaponPageViewModel : FilterablePageViewModel<WeaponViewModel>
{
    private readonly WeaponStoryService _storyService;
    private readonly WeaponGuideService _guideService;
    private readonly DispatcherQueue _dispatcher = DispatcherQueue.GetForCurrentThread();

    private ObservableCollection<GroupViewModel<WeaponViewModel>>? _groups;
    private WeaponViewModel? _selectedWeapon;

    public WeaponPageViewModel(WeaponStoryService storyService, WeaponGuideService guideService)
    {
        _storyService = storyService;
        _guideService = guideService;
    }

    public event Action<uint>? WeaponSelected;

    public WeaponViewModel? SelectedWeapon => _selectedWeapon;
    public string? WeaponStory { get; private set; }
    public Visibility WeaponStoryVisibility => string.IsNullOrEmpty(WeaponStory) ? Visibility.Collapsed : Visibility.Visible;
    public IReadOnlyList<WeaponRecommendAvatar> WeaponGuideBuilds { get; private set; } = [];
    public IReadOnlyList<WeaponRecommendAvatar> WeaponGuideAbyss { get; private set; } = [];
    public Visibility WeaponGuideVisibility => (WeaponGuideBuilds.Count > 0 || WeaponGuideAbyss.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmptyStateVisible => _selectedWeapon is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedWeaponVisible => _selectedWeapon is null ? Visibility.Collapsed : Visibility.Visible;

    public string RefineDesc0 => RefineDescAt(0);
    public string RefineDesc1 => RefineDescAt(1);
    public string RefineDesc2 => RefineDescAt(2);
    public string RefineDesc3 => RefineDescAt(3);
    public string RefineDesc4 => RefineDescAt(4);

    private string RefineDescAt(int index) =>
        _selectedWeapon?.RefinementDescriptions.Count > index ? _selectedWeapon.RefinementDescriptions[index] : string.Empty;

    public void Initialize(ObservableCollection<GroupViewModel<WeaponViewModel>>? groups)
    {
        _groups = groups;
        IEnumerable<WeaponViewModel> items = groups?.SelectMany(g => g.Items) ?? [];
        AvailableTokens = WeaponSearchTokens.Build(items);
        FilterTokens.Clear();
        RefreshItems();

        if (_selectedWeapon is null && CurrentItems?.Count > 0)
            SelectWeapon(CurrentItems[0]);

        NotifyListChanged();
        OnPropertyChanged(nameof(AvailableTokens));
    }

    public void SelectWeapon(WeaponViewModel? vm)
    {
        if (vm is null && _selectedWeapon is not null) return;

        _selectedWeapon = vm;
        WeaponStory = null;
        WeaponGuideBuilds = [];
        WeaponGuideAbyss = [];
        NotifySelectionChanged();

        if (vm is not null)
        {
            WeaponSelected?.Invoke(vm.Source.Id);
            _ = FetchWeaponStoryAsync(vm.Source.Id);
            _ = FetchWeaponGuidesAsync(vm.Source.Id);
        }
    }

    protected override IReadOnlyList<WeaponViewModel>? GetSourceItems() =>
        _groups?.SelectMany(g => g.Items).ToList();

    protected override string MatchValue(WeaponViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.Weapon        => vm.Source.Name,
        SearchTokenKind.WeaponType    => vm.Source.Type,
        SearchTokenKind.ItemQuality   => CommonSearchTokens.RankLabel(vm.Source.Rank),
        SearchTokenKind.FightProperty => vm.SubPropName,
        _                             => string.Empty,
    };

    private async Task FetchWeaponStoryAsync(uint weaponId)
    {
        var story = await _storyService.FetchStoryAsync(weaponId).ConfigureAwait(false);
        _dispatcher.TryEnqueue(() =>
        {
            if (_selectedWeapon?.Source.Id != weaponId) return;
            WeaponStory = story;
            OnPropertyChanged(nameof(WeaponStory));
            OnPropertyChanged(nameof(WeaponStoryVisibility));
        });
    }

    private async Task FetchWeaponGuidesAsync(uint weaponId)
    {
        var guides = await _guideService.FetchGuidesAsync(weaponId).ConfigureAwait(false);
        _dispatcher.TryEnqueue(() =>
        {
            if (_selectedWeapon?.Source.Id != weaponId) return;
            WeaponGuideBuilds = [.. (guides?.Builds ?? []).Select(ToRecommendAvatar)];
            WeaponGuideAbyss  = [.. (guides?.Abyss ?? []).Select(ToRecommendAvatar)];
            OnPropertyChanged(nameof(WeaponGuideBuilds));
            OnPropertyChanged(nameof(WeaponGuideAbyss));
            OnPropertyChanged(nameof(WeaponGuideVisibility));
        });
    }

    private static WeaponRecommendAvatar ToRecommendAvatar(WeaponGuideService.AvatarGuide g) =>
        new(StaticResources.AvatarIcon(g.Icon), StaticResources.GetQualityBitmap(g.Rank), g.Name);

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedWeapon));
        OnPropertyChanged(nameof(EmptyStateVisible));
        OnPropertyChanged(nameof(SelectedWeaponVisible));
        OnPropertyChanged(nameof(WeaponStory));
        OnPropertyChanged(nameof(WeaponStoryVisibility));
        OnPropertyChanged(nameof(WeaponGuideBuilds));
        OnPropertyChanged(nameof(WeaponGuideAbyss));
        OnPropertyChanged(nameof(WeaponGuideVisibility));
        OnPropertyChanged(nameof(RefineDesc0));
        OnPropertyChanged(nameof(RefineDesc1));
        OnPropertyChanged(nameof(RefineDesc2));
        OnPropertyChanged(nameof(RefineDesc3));
        OnPropertyChanged(nameof(RefineDesc4));
    }
}