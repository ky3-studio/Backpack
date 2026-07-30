using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Weapon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class WeaponPage : UserControl
{
    public static readonly DependencyProperty WeaponGroupsProperty =
        DependencyProperty.Register(nameof(WeaponGroups),
            typeof(ObservableCollection<GroupViewModel<WeaponViewModel>>), typeof(WeaponPage),
            new PropertyMetadata(null, OnWeaponGroupsChanged));

    public ObservableCollection<GroupViewModel<WeaponViewModel>>? WeaponGroups
    {
        get => (ObservableCollection<GroupViewModel<WeaponViewModel>>?)GetValue(WeaponGroupsProperty);
        set => SetValue(WeaponGroupsProperty, value);
    }

    internal WeaponPageViewModel ViewModel { get; }

    public WeaponPage()
    {
        ViewModel = App.Services.GetRequiredService<WeaponPageViewModel>();
        ViewModel.WeaponSelected += OnViewModelWeaponSelected;
        InitializeComponent();
        LevelSlider.Initialize(App.Services.GetRequiredService<WeaponMetaService>());
        ContentScroller.AddHandler(PointerPressedEvent,
            new PointerEventHandler(OnPagePointerPressed), handledEventsToo: true);
    }

    private static void OnWeaponGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WeaponPage page)
            page.ViewModel.Initialize((ObservableCollection<GroupViewModel<WeaponViewModel>>?)e.NewValue);
    }

    private void OnViewModelWeaponSelected(uint weaponId)
    {
        LevelSlider.SetWeapon(weaponId);
    }

    private void OnWeaponSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectWeapon(e.AddedItems.OfType<WeaponViewModel>().FirstOrDefault());
    }

    private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (SearchBox.FocusState == FocusState.Unfocused) return;
        if (e.OriginalSource is DependencyObject src && UiHelper.IsChildOf(src, SearchBox)) return;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => _ = FocusManager.TryFocusAsync(ContentScroller, FocusState.Pointer));
    }
}
