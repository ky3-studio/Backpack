using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class TalentSlotViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public string     Name                 { get; }
    public bool       Active               { get; }
    public double     ActiveOpacity        { get; }
    public Visibility LockedVisibility     { get; }
    public string     Description          { get; }
    public string?    ExtraLevelText       { get; }
    public Visibility ExtraLevelVisibility { get; }

    public TalentSlotViewModel(string name, string icon, bool active, string rawDescription, string? extraLevelText)
    {
        Name                 = name;
        Active               = active;
        ActiveOpacity        = active ? 1.0 : 0.5;
        LockedVisibility     = active ? Visibility.Collapsed : Visibility.Visible;
        Description          = rawDescription;
        ExtraLevelText       = extraLevelText;
        ExtraLevelVisibility = string.IsNullOrEmpty(extraLevelText) ? Visibility.Collapsed : Visibility.Visible;
        if (!string.IsNullOrEmpty(icon))
            GfxLoader.BeginLoad(StaticResources.TalentIcon(icon), this);
    }
}
