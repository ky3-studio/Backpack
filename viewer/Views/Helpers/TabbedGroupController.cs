using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views.Helpers;

internal sealed class TabbedGroupController<TGroup> : IDisposable
    where TGroup : GroupViewModel
{
    private readonly Pivot                    _pivot;
    private readonly Action                   _updateBindings;
    private ObservableCollection<TGroup>?     _source;

    public TGroup? SelectedGroup { get; private set; }

    public TabbedGroupController(Pivot pivot, Action updateBindings)
    {
        _pivot          = pivot;
        _updateBindings = updateBindings;
    }

    public void Bind(ObservableCollection<TGroup>? source)
    {
        if (_source is not null)
            _source.CollectionChanged -= OnCollectionChanged;

        _source = source;

        if (_source is null) return;
        _source.CollectionChanged += OnCollectionChanged;
        SelectedGroup = _source.Count > 0 ? _source[0] : null;
        _updateBindings();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_source is not { Count: > 0 }) return;
        var header = (_pivot.SelectedItem as GroupViewModel)?.Header;
        var target = (header is not null ? _source.FirstOrDefault(g => g.Header == header) : null)
                     ?? _source[0];
        SelectedGroup = target;
        _updateBindings();
    }

    public void OnTabSelectionChanged(SelectionChangedEventArgs e)
    {
        if (_pivot.SelectedItem is TGroup grp)
        {
            SelectedGroup = grp;
            _updateBindings();
        }
    }

    public void Dispose()
    {
        if (_source is not null)
            _source.CollectionChanged -= OnCollectionChanged;
    }
}
