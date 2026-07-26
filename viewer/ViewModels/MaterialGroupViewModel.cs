namespace Backpack.Viewer.ViewModels;

public sealed class MaterialGroupViewModel : GroupViewModel<MaterialViewModel>
{
    public MaterialGroupViewModel(string header, IReadOnlyList<MaterialViewModel> items)
        : base(header, items) { }
}
