using Backpack.Viewer.Models;
using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class AssetViewModel : SimpleItemViewModel
{
    public AssetViewModel(MaterialEntry entry, AssetMetaService meta)
        : base(entry.Name, entry.Count, meta.GetMeta(entry.Id)) { }
}
