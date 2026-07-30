using Backpack.Viewer.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.ViewModels;

public sealed class MonsterResistViewModel
{
    public MonsterResistViewModel(string element, float value)
    {
        Icon  = StaticResources.ElementResistBitmap(element);
        Value = $"{value * 100:0}%";
    }

    public BitmapImage? Icon  { get; }
    public string       Value { get; }
}
