using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer.Views;

public sealed partial class AvatarPage : UserControl
{
    public static readonly DependencyProperty AvatarsProperty =
        DependencyProperty.Register(nameof(Avatars), typeof(ObservableCollection<AvatarViewModel>), typeof(AvatarPage),
            new PropertyMetadata(null, OnAvatarsChanged));

    public static readonly DependencyProperty SelectedAvatarProperty =
        DependencyProperty.Register(nameof(SelectedAvatar), typeof(AvatarViewModel), typeof(AvatarPage),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TotalCountTextProperty =
        DependencyProperty.Register(nameof(TotalCountText), typeof(string), typeof(AvatarPage),
            new PropertyMetadata(string.Empty));

    public ObservableCollection<AvatarViewModel> Avatars
    {
        get => (ObservableCollection<AvatarViewModel>)GetValue(AvatarsProperty);
        set => SetValue(AvatarsProperty, value);
    }

    public AvatarViewModel? SelectedAvatar
    {
        get => (AvatarViewModel?)GetValue(SelectedAvatarProperty);
        set => SetValue(SelectedAvatarProperty, value);
    }

    public string TotalCountText
    {
        get => (string)GetValue(TotalCountTextProperty);
        private set => SetValue(TotalCountTextProperty, value);
    }

    public AvatarPage() => InitializeComponent();

    private static void OnAvatarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var page = (AvatarPage)d;
        if (e.OldValue is ObservableCollection<AvatarViewModel> old)
            old.CollectionChanged -= page.OnCollectionChanged;
        if (e.NewValue is ObservableCollection<AvatarViewModel> col)
        {
            col.CollectionChanged += page.OnCollectionChanged;
            page.SelectedAvatar = col.Count > 0 ? col[0] : null;
        }
        page.UpdateCount();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedAvatar ??= Avatars?.Count > 0 ? Avatars[0] : null;
        UpdateCount();
    }

    private void UpdateCount()
        => TotalCountText = Avatars is { Count: > 0 } col ? $"共 {col.Count} 位角色" : string.Empty;
}
