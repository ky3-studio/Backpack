using System.Collections.ObjectModel;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Search;

internal abstract partial class FilterablePageViewModel<TItem> : ObservableObject
    where TItem : class
{
    private IReadOnlyList<TItem>? _currentItems;

    public IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; protected set; }
    public ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }

    public IReadOnlyList<TItem>? CurrentItems => _currentItems;
    public Visibility HasItems => (CurrentItems?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

    protected abstract IReadOnlyList<TItem>? GetSourceItems();
    protected abstract string MatchValue(TItem item, SearchTokenKind kind);
    protected virtual void OnItemsRefreshed() { }

    [RelayCommand]
    private void ApplyFilter()
    {
        RefreshItems();
        NotifyListChanged();
        OnItemsRefreshed();
    }

    protected void RefreshItems() =>
        _currentItems = SearchTokenFilter.Apply(GetSourceItems(), FilterTokens, MatchValue);

    protected virtual void NotifyListChanged()
    {
        OnPropertyChanged(nameof(CurrentItems));
        OnPropertyChanged(nameof(HasItems));
    }
}