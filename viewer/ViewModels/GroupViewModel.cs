namespace Backpack.Viewer.ViewModels;

public abstract class GroupViewModel<TItem>
{
    public string                Header { get; }
    public IReadOnlyList<TItem>  Items  { get; }

    protected GroupViewModel(string header, IReadOnlyList<TItem> items)
    {
        Header = header;
        Items  = items;
    }
}
