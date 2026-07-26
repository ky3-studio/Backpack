using System.Collections.ObjectModel;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

    private void OnItemIconFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is not Image img) return;
        img.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
            new Uri("ms-appx:///Assets/Quality/UI_ItemIcon_None.png"));
    }
}
