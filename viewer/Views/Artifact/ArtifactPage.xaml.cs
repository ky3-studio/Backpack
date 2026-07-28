using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class ArtifactPage : UserControl
{
    public static readonly DependencyProperty ArtifactsProperty =
        DependencyProperty.Register(nameof(Artifacts), typeof(ObservableCollection<ArtifactViewModel>), typeof(ArtifactPage), new PropertyMetadata(null));

    public ObservableCollection<ArtifactViewModel> Artifacts
    {
        get => (ObservableCollection<ArtifactViewModel>)GetValue(ArtifactsProperty);
        set => SetValue(ArtifactsProperty, value);
    }

    public ArtifactPage() => InitializeComponent();

    private void OnRepeaterElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FrameworkElement fe &&
            sender.ItemsSourceView?.GetAt(args.Index) is ArtifactViewModel vm)
        {
            fe.Tag          = vm;
            fe.DoubleTapped += OnCardDoubleTapped;
        }
    }

    private void OnRepeaterElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is FrameworkElement fe)
            fe.DoubleTapped -= OnCardDoubleTapped;
    }

    private void OnCardDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is ArtifactViewModel vm)
            UiHelper.ShowDetailFlyout(fe, vm.Source.Set, vm.BonusText);
    }
}
