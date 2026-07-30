using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Artifact;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class ArtifactPage : UserControl
{
    public static readonly DependencyProperty ArtifactGroupsProperty =
        DependencyProperty.Register(nameof(ArtifactGroups), typeof(ObservableCollection<GroupViewModel<ArtifactViewModel>>), typeof(ArtifactPage),
            new PropertyMetadata(null, OnArtifactGroupsChanged));

    public ObservableCollection<GroupViewModel<ArtifactViewModel>>? ArtifactGroups
    {
        get => (ObservableCollection<GroupViewModel<ArtifactViewModel>>?)GetValue(ArtifactGroupsProperty);
        set => SetValue(ArtifactGroupsProperty, value);
    }

    internal ArtifactPageViewModel ViewModel { get; } = new();

    public ArtifactPage()
    {
        InitializeComponent();
        ContentScroller.AddHandler(PointerPressedEvent,
            new PointerEventHandler(OnPagePointerPressed), handledEventsToo: true);
    }

    private static void OnArtifactGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ArtifactPage page)
            page.ViewModel.Initialize((ObservableCollection<GroupViewModel<ArtifactViewModel>>?)e.NewValue);
    }

    private void OnArtifactSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectArtifact(e.AddedItems.OfType<ArtifactViewModel>().FirstOrDefault());
    }

    private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (SearchBox.FocusState == FocusState.Unfocused) return;
        if (e.OriginalSource is DependencyObject src && UiHelper.IsChildOf(src, SearchBox)) return;
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => _ = FocusManager.TryFocusAsync(ContentScroller, FocusState.Pointer));
    }
}
