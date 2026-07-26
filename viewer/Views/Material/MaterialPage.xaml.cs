using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class MaterialPage : UserControl
{
    public static readonly DependencyProperty MaterialGroupsProperty =
        DependencyProperty.Register(nameof(MaterialGroups), typeof(ObservableCollection<MaterialGroupViewModel>), typeof(MaterialPage), new PropertyMetadata(null));

    public ObservableCollection<MaterialGroupViewModel> MaterialGroups
    {
        get => (ObservableCollection<MaterialGroupViewModel>)GetValue(MaterialGroupsProperty);
        set => SetValue(MaterialGroupsProperty, value);
    }

    public MaterialPage() => InitializeComponent();

    private void OnRepeaterElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FrameworkElement fe &&
            sender.ItemsSourceView?.GetAt(args.Index) is IIconUpdatable vm)
            fe.Tag = vm;
    }


    private async void OnItemIconFailed(object sender, ExceptionRoutedEventArgs e) =>
        await GfxLoader.HandleIconFailedAsync(sender);
}
