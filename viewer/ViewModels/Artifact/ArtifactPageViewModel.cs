using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Artifact;

internal sealed partial class ArtifactPageViewModel : ObservableObject
{
    private ObservableCollection<GroupViewModel<ArtifactViewModel>>? _groups;
    private ArtifactViewModel? _selectedArtifact;
    private IReadOnlyList<ArtifactViewModel>? _currentItems;

    public IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    public ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }

    public IReadOnlyList<ArtifactViewModel>? CurrentItems => _currentItems;
    public ArtifactViewModel? SelectedArtifact => _selectedArtifact;

    public Visibility EmptyStateVisible => _selectedArtifact is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedArtifactVisible => _selectedArtifact is null ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HasItems => (CurrentItems?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

    public void Initialize(ObservableCollection<GroupViewModel<ArtifactViewModel>>? groups)
    {
        _groups = groups;
        IEnumerable<ArtifactViewModel> items = groups?.SelectMany(g => g.Items) ?? [];
        AvailableTokens = ArtifactSearchTokens.Build(items);
        FilterTokens.Clear();
        RefreshItems();
        EnsureSelection();
        NotifyListChanged();
        OnPropertyChanged(nameof(AvailableTokens));
    }

    public void SelectArtifact(ArtifactViewModel? vm)
    {
        if (vm is null && _selectedArtifact is not null) return;

        _selectedArtifact = vm;
        NotifySelectionChanged();
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        RefreshItems();
        NotifyListChanged();
        EnsureSelection();
    }

    private void EnsureSelection()
    {
        if (_selectedArtifact is not null && (_currentItems?.Contains(_selectedArtifact) ?? false)) return;

        _selectedArtifact = _currentItems?.Count > 0 ? _currentItems[0] : null;
        NotifySelectionChanged();
    }

    private void RefreshItems()
    {
        IReadOnlyList<ArtifactViewModel>? items = _groups?.SelectMany(g => g.Items).ToList();
        _currentItems = SearchTokenFilter.Apply(items, FilterTokens, MatchValue);
    }

    private static string MatchValue(ArtifactViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.ArtifactSet  => vm.Source.Set,
        SearchTokenKind.ArtifactSlot => vm.Source.Slot,
        SearchTokenKind.ItemQuality  => CommonSearchTokens.RankLabel(vm.Source.Rank),
        _                            => string.Empty,
    };

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(CurrentItems));
        OnPropertyChanged(nameof(HasItems));
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedArtifact));
        OnPropertyChanged(nameof(EmptyStateVisible));
        OnPropertyChanged(nameof(SelectedArtifactVisible));
    }
}
