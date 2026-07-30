using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class ArtifactPieceViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string                SlotName     { get; }
    public string                PieceName    { get; }
    public string                Story        { get; }
    public IReadOnlyList<string> MainStatKeys { get; }

    public Visibility StoryVisibility => string.IsNullOrEmpty(Story) ? Visibility.Collapsed : Visibility.Visible;

    public ArtifactPieceViewModel(string setName, string slot, ArtifactMetaService meta)
    {
        SlotName     = slot;
        PieceName    = meta.GetPieceName(setName, slot);
        Story        = meta.GetStory(setName, slot);
        MainStatKeys = ArtifactMainStats.ForSlot(slot);

        var icon = meta.GetIcon(setName, slot);
        if (icon is not null)
            GfxLoader.BeginLoad(icon, this);
    }
}

internal static class ArtifactMainStats
{
    public static IReadOnlyList<string> ForSlot(string slot)
    {
        if (slot == SR.SlotFlower) return [PropShortNames.Hp];
        if (slot == SR.SlotPlume)  return [PropShortNames.Attack];
        if (slot == SR.SlotSands)
            return [PropShortNames.HpPercent, PropShortNames.AttackPercent, PropShortNames.DefensePercent, PropShortNames.ElementMastery, PropShortNames.ChargeEfficiency];
        if (slot == SR.SlotGoblet)
            return [PropShortNames.HpPercent, PropShortNames.AttackPercent, PropShortNames.DefensePercent, PropShortNames.ElementMastery, PropShortNames.FireDmg, PropShortNames.WaterDmg, PropShortNames.ElecDmg, PropShortNames.IceDmg, PropShortNames.WindDmg, PropShortNames.RockDmg, PropShortNames.GrassDmg, PropShortNames.PhysicalDmg];
        if (slot == SR.SlotCirclet)
            return [PropShortNames.HpPercent, PropShortNames.AttackPercent, PropShortNames.DefensePercent, PropShortNames.ElementMastery, PropShortNames.CritRate, PropShortNames.CritDmg, PropShortNames.HealBonus];
        return [];
    }
}
