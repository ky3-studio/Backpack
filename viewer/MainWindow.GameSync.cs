using System;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace Backpack.Viewer;

public sealed partial class MainWindow
{
    private bool _drawerOpen;

    private void OnToggleDrawer(object sender, RoutedEventArgs e)
    {
        if (_drawerOpen) CloseDrawer();
        else OpenDrawer();
    }

    private void OnScrimTapped(object sender, TappedRoutedEventArgs e) => CloseDrawer();

    private void OpenDrawer()
    {
        _drawerOpen            = true;
        DrawerScrim.Visibility = Visibility.Visible;
        DrawerHost.Visibility  = Visibility.Visible;
        AnimateDrawer(open: true);
    }

    private void CloseDrawer()
    {
        if (!_drawerOpen) return;
        _drawerOpen = false;
        AnimateDrawer(open: false, onComplete: () =>
        {
            DrawerScrim.Visibility = Visibility.Collapsed;
            DrawerHost.Visibility  = Visibility.Collapsed;
        });
    }

    private void AnimateDrawer(bool open, Action? onComplete = null)
    {
        var storyboard = new Storyboard();

        var slide = new DoubleAnimation
        {
            From           = open ? 400 : 0,
            To             = open ? 0 : 400,
            Duration       = TimeSpan.FromMilliseconds(220),
            EasingFunction = new CubicEase { EasingMode = open ? EasingMode.EaseOut : EasingMode.EaseIn },
        };
        Storyboard.SetTarget(slide, DrawerSlide);
        Storyboard.SetTargetProperty(slide, "X");
        storyboard.Children.Add(slide);

        var scrimFade = new DoubleAnimation
        {
            From     = open ? 0 : 1,
            To       = open ? 1 : 0,
            Duration = TimeSpan.FromMilliseconds(220),
        };
        Storyboard.SetTarget(scrimFade, DrawerScrim);
        Storyboard.SetTargetProperty(scrimFade, "Opacity");
        storyboard.Children.Add(scrimFade);

        if (onComplete is not null)
            storyboard.Completed += (_, _) => onComplete();
        storyboard.Begin();
    }

    private void OnDrawerPickPath(object sender, RoutedEventArgs e) => PickGamePath();

    private async void OnSyncBag(object sender, RoutedEventArgs e)
    {
        CloseDrawer();

        var gamePath = ViewModel.GamePathService.SelectedPath;
        if (gamePath is null) return;

        var outputDir    = GameLaunchService.GetOutputDir(gamePath);
        var syncStartUtc = DateTime.UtcNow;

        ViewModel.IsLaunching = true;
        ViewModel.StatusText  = SR.StatusLaunching;

        try
        {
            _launchedPid            = await GameLaunchService.LaunchAsync(gamePath);
            ViewModel.IsGameRunning = true;
        }
        catch (Exception ex)
        {
            ViewModel.StatusText  = ex.Message;
            ViewModel.IsLaunching = false;
            return;
        }

        _syncDialog = new ContentDialog
        {
            XamlRoot        = Content.XamlRoot,
            Title           = SR.SyncBagDialogTitle,
            CloseButtonText = SR.SyncBagDialogCancel,
            DefaultButton   = ContentDialogButton.None,
            Content         = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing     = 12,
                Children    =
                {
                    new ProgressRing { IsActive = true, Width = 24, Height = 24 },
                    new TextBlock
                    {
                        Text              = SR.SyncBagDialogWaiting,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily        = (Microsoft.UI.Xaml.Media.FontFamily)Application.Current.Resources["AppFontFamily"]
                    }
                }
            }
        };

        await _syncDialog.ShowAsync();
        _syncDialog           = null;
        ViewModel.IsLaunching = false;

        KillLaunchedGame();

        if (ViewModel.ImportFromOutput(outputDir, syncStartUtc))
            ViewModel.StatusText = $"{SR.StatusReceived} · {DateTime.Now:HH:mm:ss}";
    }

    private void KillLaunchedGame()
    {
        if (_launchedPid <= 0) return;
        try
        {
            var p = System.Diagnostics.Process.GetProcessById(_launchedPid);
            if (!p.HasExited) p.Kill();
        }
        catch { }
        finally
        {
            _launchedPid            = 0;
            ViewModel.IsGameRunning = false;
        }
    }

    private void OnKillGame(object sender, RoutedEventArgs e) => KillLaunchedGame();
}
