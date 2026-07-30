using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Monster;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

[DependencyProperty<ObservableCollection<MonsterViewModel>>("Monsters", PropertyChangedCallbackName = "OnMonstersChanged")]
public sealed partial class MonsterPage : UserControl
{

    internal MonsterPageViewModel ViewModel { get; } = new();

    public MonsterPage()
    {
        InitializeComponent();
        ContentScroller.AddHandler(PointerPressedEvent,
            new PointerEventHandler(OnPagePointerPressed), handledEventsToo: true);
    }

    private static void OnMonstersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MonsterPage page)
            page.ViewModel.Initialize((ObservableCollection<MonsterViewModel>?)e.NewValue);
    }

    private void OnMonsterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectMonster(e.AddedItems.OfType<MonsterViewModel>().FirstOrDefault());
    }

    private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (SearchBox.FocusState == FocusState.Unfocused) return;
        if (e.OriginalSource is DependencyObject src && UiHelper.IsChildOf(src, SearchBox)) return;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => _ = FocusManager.TryFocusAsync(ContentScroller, FocusState.Pointer));
    }
}
