namespace Backpack.Viewer.ViewModels;

public sealed class GadgetGroupViewModel : GroupViewModel<GadgetViewModel>
{
    public GadgetGroupViewModel(string header, IReadOnlyList<GadgetViewModel> items)
        : base(header, items) { }
}
