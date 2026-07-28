using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

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
        var gadgetMeta   = new GadgetMetaService();
        var assetMeta    = new AssetMetaService();
        var avatarMeta   = new AvatarMetaService();
        var avatarDetail = new AvatarDetailService();
        ViewModel = new MainViewModel(DispatcherQueue.GetForCurrentThread(), gps, meta, foodMeta, weaponMeta, artifactMeta, gadgetMeta, assetMeta, avatarMeta, avatarDetail, _db);
        InitializeComponent();

        Title = $"Backpack {AppVersion.Value}";
        AppTitleText.Text = $"Backpack {AppVersion.Value}";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "logo.ico"));
        const int W = 1280, H = 800;
        AppWindow.Resize(new SizeInt32(W, H));
        var workArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
        AppWindow.Move(new PointInt32(workArea.X + (workArea.Width - W) / 2, workArea.Y + (workArea.Height - H) / 2));

        _pipe.PacketReceived += ViewModel.OnPacketReceived;
        _ = _pipe.RunAsync(_cts.Token);

        ViewModel.DataReceived += () =>
        {
            _syncDialog?.Hide();
            KillLaunchedGame();
        };

        _gameMonitor.Tick += (_, _) => ViewModel.IsGameRunning = GameLaunchService.IsGameRunning();
        _gameMonitor.Start();

        SetupPageControl.AddPathRequested += (_, _) => _ = PickGamePathAsync();

        Closed += (_, _) => Dispose();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _db.Dispose();
        _gameMonitor.Stop();
    }
}
