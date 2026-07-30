using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Monster;

internal sealed class MonsterPageViewModel : FilterablePageViewModel<MonsterViewModel>
{
    private ObservableCollection<MonsterViewModel>? _source;
    private MonsterViewModel? _selectedMonster;

    public MonsterViewModel? SelectedMonster => _selectedMonster;
    public Visibility EmptyStateVisible => _selectedMonster is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedMonsterVisible => _selectedMonster is null ? Visibility.Collapsed : Visibility.Visible;

    public void Initialize(ObservableCollection<MonsterViewModel>? source)
    {
        if (_source is not null)
            _source.CollectionChanged -= OnSourceChanged;
        _source = source;
        if (_source is not null)
            _source.CollectionChanged += OnSourceChanged;

        Rebuild();
    }

    public void SelectMonster(MonsterViewModel? vm)
    {
        if (vm is null && _selectedMonster is not null) return;
        _selectedMonster = vm;
        NotifySelectionChanged();
    }

    protected override IReadOnlyList<MonsterViewModel>? GetSourceItems() => _source?.ToList();

    protected override string MatchValue(MonsterViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.Monster     => vm.Name,
        SearchTokenKind.MonsterType => vm.TypeName,
        _                           => string.Empty,
    };

    protected override void OnItemsRefreshed() => EnsureSelection();

    private void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        IEnumerable<MonsterViewModel> items = _source ?? [];
        AvailableTokens = MonsterSearchTokens.Build(items);
        RefreshItems();
        NotifyListChanged();
        EnsureSelection();
        OnPropertyChanged(nameof(AvailableTokens));
    }

    private void EnsureSelection()
    {
        if (_selectedMonster is not null && (CurrentItems?.Contains(_selectedMonster) ?? false)) return;
        _selectedMonster = CurrentItems?.Count > 0 ? CurrentItems[0] : null;
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedMonster));
        OnPropertyChanged(nameof(EmptyStateVisible));
        OnPropertyChanged(nameof(SelectedMonsterVisible));
    }
}
