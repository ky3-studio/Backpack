using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class SaintCardViewModel : SimpleItemViewModel
{
    public uint Id { get; }

    public SaintCardViewModel(uint id, string name, ulong count, int rank, string icon)
        : base(name, count, (StaticResources.SaintCardIcon(icon), rank))
    {
        Id = id;
    }
}
