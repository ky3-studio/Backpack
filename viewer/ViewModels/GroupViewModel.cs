namespace Backpack.Viewer.ViewModels;

public abstract class GroupViewModel
{
    public string Header { get; }
    protected GroupViewModel(string header) => Header = header;
}

public sealed class GroupViewModel<TItem> : GroupViewModel
{
    public IReadOnlyList<TItem> Items { get; }
    public GroupViewModel(string header, IReadOnlyList<TItem> items) : base(header) => Items = items;
}
