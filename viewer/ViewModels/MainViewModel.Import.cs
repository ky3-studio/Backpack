using Backpack.Viewer.Services;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    internal bool ImportFromOutput(string outputDir, DateTime freshUtcThreshold)
    {
        var data = BackpackOutputReader.Read(outputDir, freshUtcThreshold);
        if (!data.HasAny) return false;

        if (data.Weapon   is not null) ApplyWeapon(data.Weapon);
        if (data.Artifact is not null) ApplyArtifact(data.Artifact);
        if (data.Material is not null) ApplyMaterial(data.Material);
        if (data.Prop     is not null) ApplyProp(data.Prop);
        if (data.Avatar   is not null) ApplyAvatar(data.Avatar);

        return true;
    }
}
