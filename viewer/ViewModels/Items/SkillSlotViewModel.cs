using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class SkillSlotViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string Name                { get; }
    public int    Level               { get; }
    public string LevelText           { get; }
    public string TypeDisplay         { get; }
    public string Description         { get; }
    public IReadOnlyList<SkillParamRow> Params { get; }
    public Visibility DescriptionVisibility { get; }
    public Visibility ParamsVisibility      { get; }

    public SkillSlotViewModel(string name, string icon, int level, string type,
        string description = "", IReadOnlyList<SkillParamRow>? skillParams = null)
    {
        Name        = name;
        Level       = level;
        LevelText   = level > 0 ? $"{Localized.Get("LevelPrefix")}{level}" : string.Empty;
        TypeDisplay = type switch
        {
            "normal" => Localized.Get("SkillTypeNormal"),
            "skill"  => Localized.Get("SkillTypeSkill"),
            "burst"  => Localized.Get("SkillTypeBurst"),
            "dash"   => Localized.Get("SkillTypeDash"),
            _        => type
        };
        Description           = description;
        Params                = skillParams ?? [];
        DescriptionVisibility = (!string.IsNullOrEmpty(description)).ToVisibility();
        ParamsVisibility      = (Params.Count > 0).ToVisibility();
        if (!string.IsNullOrEmpty(icon))
            GfxLoader.BeginLoad(StaticResources.SkillIcon(icon), this);
    }
}
