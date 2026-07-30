using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Artifact;

internal sealed class ArtifactPageViewModel : FilterablePageViewModel<ArtifactSetViewModel>
{
    private ObservableCollection<ArtifactSetViewModel>? _source;
    private ArtifactSetViewModel? _selectedArtifact;

    public ArtifactSetViewModel? SelectedArtifact => _selectedArtifact;
    public Visibility EmptyStateVisible => _selectedArtifact is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedArtifactVisible => _selectedArtifact is null ? Visibility.Collapsed : Visibility.Visible;

    public void Initialize(ObservableCollection<ArtifactSetViewModel>? source)
    {
        if (_source is not null)
            _source.CollectionChanged -= OnSourceChanged;
        _source = source;
        if (_source is not null)
            _source.CollectionChanged += OnSourceChanged;

        Rebuild();
    }

    public void SelectArtifact(ArtifactSetViewModel? vm)
    {
        if (vm is null && _selectedArtifact is not null) return;
        _selectedArtifact = vm;
        NotifySelectionChanged();
    }

    protected override IReadOnlyList<ArtifactSetViewModel>? GetSourceItems() => _source?.ToList();

    protected override string MatchValue(ArtifactSetViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.ArtifactSet => vm.SetName,
        SearchTokenKind.ItemQuality => CommonSearchTokens.RankLabel(vm.Rank),
        _                           => string.Empty,
    };

    protected override void OnItemsRefreshed() => EnsureSelection();

    private void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        IEnumerable<ArtifactSetViewModel> items = _source ?? [];
        AvailableTokens = ArtifactSearchTokens.Build(items);
        RefreshItems();
        NotifyListChanged();
        EnsureSelection();
        OnPropertyChanged(nameof(AvailableTokens));
    }

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
