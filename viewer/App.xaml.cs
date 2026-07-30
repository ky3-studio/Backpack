using Backpack.Viewer.Controls;
using Backpack.Viewer.Services;
using Backpack.Viewer.Services.Story;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Weapon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer;

public sealed partial class App : Application, IDisposable
{
    public static IServiceProvider Services { get; private set; } = null!;

    private MainWindow?      _window;
    private ServiceProvider? _services;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        string[] cmdArgs = Environment.GetCommandLineArgs();
        if (cmdArgs.Length >= 3 && cmdArgs[1] == "--elevated-inject")
        {
            Environment.Exit(GameLaunchService.RunElevatedInjection(cmdArgs[2]));
            return;
        }
        GfxLoader.Initialize();
        await GfxLoader.WarmupAsync();
        _services = BuildServices();
        Services  = _services;
        var hyperLinkSvc = _services.GetRequiredService<HyperLinkService>();
        hyperLinkSvc.Load();
        MiHoYo.RegisterService(hyperLinkSvc);
        _window = new MainWindow(_services);
        _window.Activate();
    }

    private static ServiceProvider BuildServices() =>
        new ServiceCollection()
            .AddSingleton<HyperLinkService>()
            .AddSingleton<GamePathService>()
            .AddSingleton<BackpackDbService>()
            .AddSingleton<MaterialMetaService>()
            .AddSingleton<FoodMetaService>()
            .AddSingleton<WeaponMetaService>()
            .AddSingleton<ArtifactMetaService>()
            .AddSingleton<GadgetMetaService>()
            .AddSingleton<AssetMetaService>()
            .AddSingleton<AvatarMetaService>()
            .AddSingleton<AvatarDetailService>()
            .AddSingleton<MonsterMetaService>()
            .AddSingleton<System.Net.Http.HttpClient>()
            .AddSingleton<WeaponStoryService>()
            .AddSingleton<WeaponGuideService>()
            .AddTransient<WeaponPageViewModel>()
            .AddSingleton<MainViewModel>()
            .BuildServiceProvider();

    public void Dispose()
    {
        _window?.Dispose();
        _services?.Dispose();
    }
}
