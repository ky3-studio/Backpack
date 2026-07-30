using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views.Controls;

public sealed class LayoutSwitch : Segmented
{
    public const string List = nameof(List);
    public const string Grid = nameof(Grid);

    public static readonly DependencyProperty CurrentProperty =
        DependencyProperty.Register(nameof(Current), typeof(string), typeof(LayoutSwitch),
            new PropertyMetadata(List));

    public string Current
    {
        get => (string)GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    public LayoutSwitch()
    {
        Items.Add(new SegmentedItem { Tag = List, Icon = new FontIcon { Glyph = "\uE8FD" } });
        Items.Add(new SegmentedItem { Tag = Grid, Icon = new FontIcon { Glyph = "\uF0E2" } });
        SelectedIndex = 0;
        RegisterPropertyChangedCallback(SelectedIndexProperty, (s, _) =>
        {
            Current = ((LayoutSwitch)s).SelectedIndex == 0 ? List : Grid;
        });
    }
}
