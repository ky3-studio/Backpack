using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.Localization;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Avatar;

internal sealed partial class AvatarPageViewModel : ObservableObject
{
    private ObservableCollection<AvatarViewModel>? _source;
    private AvatarViewModel? _selectedAvatar;
    private IReadOnlyList<AvatarViewModel>? _currentItems;

    public IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    public ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }

    public IReadOnlyList<AvatarViewModel>? CurrentItems => _currentItems;
    public AvatarViewModel? SelectedAvatar => _selectedAvatar;

    public Visibility EmptyStateVisible => _selectedAvatar is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedAvatarVisible => _selectedAvatar is null ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HasItems => (CurrentItems?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

    public string TotalCountText => (CurrentItems?.Count ?? 0) is > 0 and var count
        ? string.Format(Localized.Get("AvatarCountFmt"), count)
        : string.Empty;

    public void Initialize(ObservableCollection<AvatarViewModel>? source)
    {
        if (_source is not null)
            _source.CollectionChanged -= OnSourceChanged;
        _source = source;
        if (_source is not null)
            _source.CollectionChanged += OnSourceChanged;

        Rebuild();
    }

    public void SelectAvatar(AvatarViewModel? vm)
    {
        if (vm is null && _selectedAvatar is not null) return;

        _selectedAvatar = vm;
        NotifySelectionChanged();
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        RefreshItems();
        NotifyListChanged();
        EnsureSelection();
    }

    private void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        IEnumerable<AvatarViewModel> items = _source ?? [];
        AvailableTokens = AvatarSearchTokens.Build(items);
        RefreshItems();
        NotifyListChanged();
        EnsureSelection();
        OnPropertyChanged(nameof(AvailableTokens));
    }

    private void EnsureSelection()
    {
        if (_selectedAvatar is not null && (_currentItems?.Contains(_selectedAvatar) ?? false)) return;

        _selectedAvatar = _currentItems?.Count > 0 ? _currentItems[0] : null;
        NotifySelectionChanged();
    }

    private void RefreshItems()
    {
        _currentItems = SearchTokenFilter.Apply(_source?.ToList(), FilterTokens, MatchValue);
    }

    private static string MatchValue(AvatarViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.Avatar      => vm.Name,
        SearchTokenKind.ElementName => vm.Element,
        SearchTokenKind.WeaponType  => vm.WeaponTypeName,
        SearchTokenKind.ItemQuality => CommonSearchTokens.RankLabel(vm.Rarity),
        _                           => string.Empty,
    };

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(CurrentItems));
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(TotalCountText));
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedAvatar));
        OnPropertyChanged(nameof(EmptyStateVisible));
        OnPropertyChanged(nameof(SelectedAvatarVisible));
    }
}
