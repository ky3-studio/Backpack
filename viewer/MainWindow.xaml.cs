using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.Storage.Pickers;

namespace Backpack.Viewer;

public sealed partial class MainWindow : Window, IDisposable
{
    private readonly PipeListenerService     _pipe        = new();
    private readonly CancellationTokenSource _cts         = new();
    private readonly BackpackDbService       _db          = new();
    private readonly DispatcherTimer         _gameMonitor = new() { Interval = TimeSpan.FromSeconds(2) };
    private ContentDialog?                   _syncDialog;
    private int                              _launchedPid;

    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        var gps          = new GamePathService();
        var meta         = new MaterialMetaService();
        var foodMeta     = new FoodMetaService();
        var weaponMeta   = new WeaponMetaService();
        var artifactMeta = new ArtifactMetaService();
        ViewModel = new MainViewModel(DispatcherQueue.GetForCurrentThread(), gps, meta, foodMeta, weaponMeta, artifactMeta, _db);
        InitializeComponent();

        Title = $"Backpack {AppVersion.Value}";
        AppTitleText.Text = $"Backpack {AppVersion.Value}";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "logo.ico"));
        AppWindow.Resize(new SizeInt32(1280, 800));

        _pipe.PacketReceived += ViewModel.OnPacketReceived;
        _ = _pipe.RunAsync(_cts.Token);

        ViewModel.DataReceived += () =>
        {
            _syncDialog?.Hide();
            KillLaunchedGame();
        };

        _gameMonitor.Tick += (_, _) =>
            ViewModel.IsGameRunning = System.Diagnostics.Process.GetProcessesByName("YuanShen").Length > 0;
        _gameMonitor.Start();

        SetupPageControl.AddPathRequested += (_, _) => _ = PickGamePathAsync();

        Closed += (_, _) => Dispose();
    }

    private async void OnPickGamePath(object sender, RoutedEventArgs e) => await PickGamePathAsync();

    private async Task PickGamePathAsync()
    {
        ViewModel.SetupError = string.Empty;
        var picker = new FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(
            picker,
            WinRT.Interop.WindowNative.GetWindowHandle(this));
        picker.FileTypeFilter.Add(".exe");
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        if (!ViewModel.GamePathService.TryAdd(file.Path))
            ViewModel.SetupError = Localized.Get("PathInvalidMsg");
    }

    private async void OnSyncBag(object sender, RoutedEventArgs e)
    {
        var gamePath = ViewModel.GamePathService.SelectedPath;
        if (gamePath is null) return;

        ViewModel.IsLaunching = true;
        ViewModel.StatusText  = Localized.Get("StatusLaunching");

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
            Title           = Localized.Get("SyncBagDialogTitle"),
            CloseButtonText = Localized.Get("SyncBagDialogCancel"),
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
                        Text              = Localized.Get("SyncBagDialogWaiting"),
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

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _db.Dispose();
        _gameMonitor.Stop();
    }
}
