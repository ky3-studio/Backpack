using Backpack.Viewer.Models;
using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MaterialViewModel : SimpleItemViewModel
{
    public MaterialViewModel(MaterialEntry entry, MaterialMetaService meta)
        : base(entry.Name, entry.Count, meta.GetMeta(entry.Id)) { }
}
