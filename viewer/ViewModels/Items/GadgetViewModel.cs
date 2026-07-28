using Backpack.Viewer.Models;
using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class GadgetViewModel : SimpleItemViewModel
{
    public GadgetViewModel(MaterialEntry entry, GadgetMetaService meta)
        : base(entry.Name, entry.Count, meta.GetMeta(entry.Id)) { }
}
