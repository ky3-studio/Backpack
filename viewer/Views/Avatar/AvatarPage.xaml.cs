using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Avatar;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class AvatarPage : UserControl
{
    public static readonly DependencyProperty AvatarsProperty =
        DependencyProperty.Register(nameof(Avatars), typeof(ObservableCollection<AvatarViewModel>), typeof(AvatarPage),
            new PropertyMetadata(null, OnAvatarsChanged));

    public ObservableCollection<AvatarViewModel>? Avatars
    {
        get => (ObservableCollection<AvatarViewModel>?)GetValue(AvatarsProperty);
        set => SetValue(AvatarsProperty, value);
    }

    internal AvatarPageViewModel ViewModel { get; } = new();

    public AvatarPage()
    {
        InitializeComponent();
        ContentScroller.AddHandler(PointerPressedEvent,
            new PointerEventHandler(OnPagePointerPressed), handledEventsToo: true);
    }

    private static void OnAvatarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarPage page)
            page.ViewModel.Initialize((ObservableCollection<AvatarViewModel>?)e.NewValue);
    }

    private void OnAvatarSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectAvatar(e.AddedItems.OfType<AvatarViewModel>().FirstOrDefault());
    }

    private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (SearchBox.FocusState == FocusState.Unfocused) return;
        if (e.OriginalSource is DependencyObject src && UiHelper.IsChildOf(src, SearchBox)) return;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => _ = FocusManager.TryFocusAsync(ContentScroller, FocusState.Pointer));
    }
}
