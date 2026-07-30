using Backpack.Viewer;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MonsterTipImageViewModel : ObservableObject, IIconUpdatable
{
    [ObservableProperty]
    private BitmapImage? _iconSource;

    public MonsterTipImageViewModel(string name) =>
        GfxLoader.BeginLoad(StaticResources.TutorialIcon(name), this);
}

public sealed class MonsterTipViewModel
{
    public MonsterTipViewModel(MonsterMetaService.MonsterTip tip)
    {
        Images      = [.. tip.Images.Select(name => new MonsterTipImageViewModel(name))];
        Description = tip.Description;
    }

    public IReadOnlyList<MonsterTipImageViewModel> Images      { get; }
    public string                                  Description { get; }

    public Visibility ImagesVisibility      => (Images.Count > 0).ToVisibility();
    public Visibility DescriptionVisibility => (!string.IsNullOrEmpty(Description)).ToVisibility();
}
