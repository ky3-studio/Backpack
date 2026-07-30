using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using Windows.Graphics;

namespace Backpack.Viewer;

public sealed partial class MainWindow : Window, IDisposable
{
    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);
    private readonly PipeListenerService     _pipe        = new();
    private readonly CancellationTokenSource _cts         = new();
    private readonly DispatcherTimer         _gameMonitor = new() { Interval = TimeSpan.FromSeconds(2) };
    private ContentDialog?                   _syncDialog;
    private int                              _launchedPid;

    public MainViewModel ViewModel { get; }

    public MainWindow(IServiceProvider services)
    {
        ViewModel = services.GetRequiredService<MainViewModel>();
        InitializeComponent();

        Title = $"Backpack {AppVersion.Value}";
        AppTitleText.Text = $"Backpack {AppVersion.Value}";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarDragRegion);
        AppWindow.SetIcon(System.IO.Path.Combine(StaticResources.AssetsDir, "UI", "logo.ico"));
        const int logW = 1280, logH = 800;
        var    hwnd  = WinRT.Interop.WindowNative.GetWindowHandle(this);
        double scale = GetDpiForWindow(hwnd) / 96.0;
        int    physW = (int)(logW * scale);
        int    physH = (int)(logH * scale);
        AppWindow.Resize(new SizeInt32(physW, physH));
        var workArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
        AppWindow.Move(new PointInt32(
            workArea.X + (workArea.Width  - physW) / 2,
            workArea.Y + (workArea.Height - physH) / 2));

        _pipe.PacketReceived += ViewModel.OnPacketReceived;
        _ = _pipe.RunAsync(_cts.Token);

        ViewModel.SyncCompleted += () =>
        {
            _syncDialog?.Hide();
            KillLaunchedGame();
        };

        _gameMonitor.Tick += (_, _) =>
        {
            ViewModel.IsGameRunning = GameLaunchService.IsGameRunning();
            if (_launchedPid > 0)
            {
                bool gone;
                try   { gone = System.Diagnostics.Process.GetProcessById(_launchedPid).HasExited; }
                catch { gone = true; }
                if (gone) { _syncDialog?.Hide(); KillLaunchedGame(); }
            }
        };
        _gameMonitor.Start();

        SetupPageControl.AddPathRequested += (_, _) => PickGamePath();

        NavView.Loaded += (_, _) => NavView.SelectedItem = NavWeapon;

        Closed += (_, _) => Dispose();
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        PageWeapon.Visibility   = Visibility.Collapsed;
        PageAvatar.Visibility   = Visibility.Collapsed;
        PageArtifact.Visibility = Visibility.Collapsed;
        PageMonster.Visibility  = Visibility.Collapsed;
        PageMaterial.Visibility = Visibility.Collapsed;
        PageFood.Visibility     = Visibility.Collapsed;
        PageGadget.Visibility   = Visibility.Collapsed;
        PageAsset.Visibility    = Visibility.Collapsed;

        if (e.IsSettingsSelected)
            return;

        if (e.SelectedItem is NavigationViewItem item)
        {
            _ = item.Name switch
            {
                nameof(NavWeapon)   => PageWeapon.Visibility   = Visibility.Visible,
                nameof(NavAvatar)   => PageAvatar.Visibility   = Visibility.Visible,
                nameof(NavArtifact) => PageArtifact.Visibility = Visibility.Visible,
                nameof(NavMonster)  => PageMonster.Visibility  = Visibility.Visible,
                nameof(NavMaterial) => PageMaterial.Visibility = Visibility.Visible,
                nameof(NavFood)     => PageFood.Visibility     = Visibility.Visible,
                nameof(NavGadget)   => PageGadget.Visibility   = Visibility.Visible,
                nameof(NavAsset)    => PageAsset.Visibility    = Visibility.Visible,
                _                   => Visibility.Collapsed,
            };
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _gameMonitor.Stop();
    }
}
