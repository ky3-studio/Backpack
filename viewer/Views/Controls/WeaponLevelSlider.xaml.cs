using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Backpack.Viewer.Views.Controls;

public sealed partial class WeaponLevelSlider : UserControl
{
    private static readonly int[] Breakpoints = [20, 40, 50, 60, 70, 80];

    private WeaponMetaService? _meta;

    private uint _weaponId;
    private int  _level    = 90;
    private bool _promoted = false;
    private bool _suspendEvents = false;

    public WeaponLevelSlider()
    {
        _suspendEvents = true;
        InitializeComponent();
        _suspendEvents = false;
        SubPropRow.Visibility = Visibility.Collapsed;
    }

    public void Initialize(WeaponMetaService meta)
    {
        _meta = meta;
    }

    public void SetWeapon(uint weaponId, int initialLevel = 90, bool initialPromoted = false)
    {
        _weaponId = weaponId;
        _level    = Math.Clamp(initialLevel, 1, 90);
        _promoted = initialPromoted;
        Refresh();
    }

    private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_suspendEvents) return;
        var lvl = Math.Clamp((int)e.NewValue, 1, 90);
        if (_level == lvl) return;
        _level = lvl;
        if (!Breakpoints.Contains(_level)) _promoted = false;
        Refresh();
    }

    private void OnPromotedChecked(object sender, RoutedEventArgs e)
    {
        if (_suspendEvents) return;
        _promoted = true;
        Refresh();
    }

    private void OnPromotedUnchecked(object sender, RoutedEventArgs e)
    {
        if (_suspendEvents) return;
        _promoted = false;
        Refresh();
    }

    private void Refresh()
    {
        if (_meta is null) return;

        _suspendEvents = true;

        var promote = CalcPromote(_level, _promoted);
        var (atk, sub) = _meta.CalcStats(_weaponId, _level, promote);

        LevelTextBlock.Text    = $"{SR.LevelPrefix}{_level}";
        AtkTextBlock.Text      = atk > 0 ? atk.ToString() : SR.StatEmptyValue;
        ValueSlider.Value      = _level;
        PromotedCheckBox.IsChecked = _promoted;
        PromotedCheckBox.Visibility = Breakpoints.Contains(_level) ? Visibility.Visible : Visibility.Collapsed;

        if (!string.IsNullOrEmpty(sub))
        {
            SubPropNameBlock.Text  = _meta.GetSubPropName(_weaponId);
            SubTextBlock.Text      = sub;
            SubPropRow.Visibility  = Visibility.Visible;
        }
        else
        {
            SubPropRow.Visibility  = Visibility.Collapsed;
        }

        _suspendEvents = false;
    }

    private static int CalcPromote(int level, bool promoted)
    {
        if (level <= 20) return promoted ? 1 : 0;
        if (level <= 40) return promoted ? 2 : 1;
        if (level <= 50) return promoted ? 3 : 2;
        if (level <= 60) return promoted ? 4 : 3;
        if (level <= 70) return promoted ? 5 : 4;
        if (level <= 80) return promoted ? 6 : 5;
        return 6;
    }
}
