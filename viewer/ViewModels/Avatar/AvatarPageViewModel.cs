using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.Localization;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels.Avatar;

internal sealed class AvatarPageViewModel : FilterablePageViewModel<AvatarViewModel>
{
    private ObservableCollection<AvatarViewModel>? _source;
    private AvatarViewModel? _selectedAvatar;

    public AvatarViewModel? SelectedAvatar => _selectedAvatar;
    public Visibility EmptyStateVisible => _selectedAvatar is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedAvatarVisible => _selectedAvatar is null ? Visibility.Collapsed : Visibility.Visible;

    public string TotalCountText => (CurrentItems?.Count ?? 0) is > 0 and var count
        ? string.Format(SR.AvatarCountFmt, count)
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

    protected override IReadOnlyList<AvatarViewModel>? GetSourceItems() => _source?.ToList();

    protected override string MatchValue(AvatarViewModel vm, SearchTokenKind kind) => kind switch
    {
        SearchTokenKind.Avatar      => vm.Name,
        SearchTokenKind.ElementName => vm.Element,
        SearchTokenKind.WeaponType  => vm.WeaponTypeName,
        SearchTokenKind.ItemQuality => CommonSearchTokens.RankLabel(vm.Rarity),
        _                           => string.Empty,
    };

    protected override void OnItemsRefreshed() => EnsureSelection();

    protected override void NotifyListChanged()
    {
        base.NotifyListChanged();
        OnPropertyChanged(nameof(TotalCountText));
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
        if (_selectedAvatar is not null && (CurrentItems?.Contains(_selectedAvatar) ?? false)) return;
        _selectedAvatar = CurrentItems?.Count > 0 ? CurrentItems[0] : null;
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedAvatar));
        OnPropertyChanged(nameof(EmptyStateVisible));
        OnPropertyChanged(nameof(SelectedAvatarVisible));
    }
}