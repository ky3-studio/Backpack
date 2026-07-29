using System.Collections.ObjectModel;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Backpack.Viewer.Views;

public sealed partial class ArtifactPage : UserControl, IDisposable
{
    public static readonly DependencyProperty ArtifactGroupsProperty =
        DependencyProperty.Register(nameof(ArtifactGroups), typeof(ObservableCollection<GroupViewModel<ArtifactViewModel>>), typeof(ArtifactPage),
            new PropertyMetadata(null, OnArtifactGroupsChanged));

    public ObservableCollection<GroupViewModel<ArtifactViewModel>>? ArtifactGroups
    {
        get => (ObservableCollection<GroupViewModel<ArtifactViewModel>>?)GetValue(ArtifactGroupsProperty);
        set => SetValue(ArtifactGroupsProperty, value);
    }

    public IReadOnlyList<ArtifactViewModel>? CurrentItems =>
        (_controller.SelectedGroup as GroupViewModel<ArtifactViewModel>)?.Items;

    private readonly TabbedGroupController<GroupViewModel<ArtifactViewModel>> _controller;

    public ArtifactPage()
    {
        InitializeComponent();
        _controller = new TabbedGroupController<GroupViewModel<ArtifactViewModel>>(TabPivot, () => Bindings.Update());
        SetupTemplate();
    }

    private void SetupTemplate()
    {
        // 从内联 DataTemplate 包装为 PooledElementFactory（元素回收）
        // clearDataContextOnRecycle=false：x:Bind 模板 DataContext 从旧值直接更新为新值，不经过 null，避免 NRE 崩溃
        var template = (DataTemplate)TheRepeater.ItemTemplate!;
        TheRepeater.ItemTemplate = new PooledElementFactory(template, clearDataContextOnRecycle: false);
    }

    private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        // 使用 IElementFactory 时 ItemsRepeater 不自动设置 DataContext，需手动绑定
        if (args.Element is FrameworkElement fe)
            fe.DataContext = sender.ItemsSourceView?.GetAt(args.Index);
    }

    private static void OnArtifactGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ArtifactPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<ArtifactViewModel>>?)e.NewValue);
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e) =>
        _controller.OnTabSelectionChanged(e);

    private void OnCardDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is ArtifactViewModel vm)
            UiHelper.ShowDetailFlyout(fe, vm.Source.Set, vm.BonusText);
    }

    public void Dispose() => _controller.Dispose();
}
