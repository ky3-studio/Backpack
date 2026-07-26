namespace Backpack.Viewer.ViewModels;

public sealed class FoodGroupViewModel
{
    public string                       Header { get; }
    public IReadOnlyList<FoodViewModel> Items  { get; }

    public FoodGroupViewModel(string header, IReadOnlyList<FoodViewModel> items)
    {
        Header = header;
        Items  = items;
    }
}
