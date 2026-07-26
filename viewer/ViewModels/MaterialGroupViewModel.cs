namespace Backpack.Viewer.ViewModels;

public sealed class MaterialGroupViewModel
{
    public string                           Header { get; }
    public IReadOnlyList<MaterialViewModel> Items  { get; }

    public MaterialGroupViewModel(string header, IReadOnlyList<MaterialViewModel> items)
    {
        Header = header;
        Items  = items;
    }
}
