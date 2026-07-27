namespace Backpack.Viewer.ViewModels;

public sealed class AssetGroupViewModel : GroupViewModel<AssetViewModel>
{
    public AssetGroupViewModel(string header, IReadOnlyList<AssetViewModel> items)
        : base(header, items) { }
}
