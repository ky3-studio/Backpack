using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Artifact;

internal sealed class ArtifactPageViewModel : FilterablePageViewModel<ArtifactViewModel>
{
    private ObservableCollection<GroupViewModel<ArtifactViewModel>>? _groups;
    private ArtifactViewModel? _selectedArtifact;

    public ArtifactViewModel? SelectedArtifact => _selectedArtifact;
    public Visibility EmptyStateVisible => _selectedArtifact is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedArtifactVisible => _selectedArtifact is null ? Visibility.Collapsed : Visibility.Visible;

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

    protected override IReadOnlyList<ArtifactViewModel>? GetSourceItems() =>
        _groups?.SelectMany(g => g.Items).ToList();

    protected override string MatchValue(ArtifactViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.ArtifactSet  => vm.Source.Set,
        SearchTokenKind.ArtifactSlot => vm.Source.Slot,
        SearchTokenKind.ItemQuality  => CommonSearchTokens.RankLabel(vm.Source.Rank),
        _                            => string.Empty,
    };

    protected override void OnItemsRefreshed() => EnsureSelection();

    private void EnsureSelection()
    {
        if (_selectedArtifact is not null && (CurrentItems?.Contains(_selectedArtifact) ?? false)) return;
        _selectedArtifact = CurrentItems?.Count > 0 ? CurrentItems[0] : null;
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedArtifact));
        OnPropertyChanged(nameof(EmptyStateVisible));
        OnPropertyChanged(nameof(SelectedArtifactVisible));
    }
}