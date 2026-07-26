using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class SetupPage : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(SetupPage), new PropertyMetadata(null));

    public MainViewModel? ViewModel
    {
        get => (MainViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler? AddPathRequested;

    public SetupPage() => InitializeComponent();

    private void OnAddPathButtonClick(object sender, RoutedEventArgs e) =>
        AddPathRequested?.Invoke(this, EventArgs.Empty);

    private void OnPathItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not string path || ViewModel is null) return;
        ViewModel.GamePathService.Select(path);
        ViewModel.HasSelectedPath = true;
    }

    private void OnRemovePathClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not string path || ViewModel is null) return;
        ViewModel.GamePathService.Remove(path);
        if (!ViewModel.GamePathService.HasSelection)
            ViewModel.HasSelectedPath = false;
    }
}
