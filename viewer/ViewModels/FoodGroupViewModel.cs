namespace Backpack.Viewer.ViewModels;

public sealed class FoodGroupViewModel : GroupViewModel<FoodViewModel>
{
    public FoodGroupViewModel(string header, IReadOnlyList<FoodViewModel> items)
        : base(header, items) { }
}
