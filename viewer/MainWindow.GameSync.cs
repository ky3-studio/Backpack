using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Backpack.Viewer;

public sealed partial class MainWindow
{
    private async void OnSyncBag(object sender, RoutedEventArgs e)
    {
        var gamePath = ViewModel.GamePathService.SelectedPath;
        if (gamePath is null) return;

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
