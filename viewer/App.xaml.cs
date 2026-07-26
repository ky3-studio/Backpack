using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer;

public sealed partial class App : Application
{
    private MainWindow? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        string[] cmdArgs = Environment.GetCommandLineArgs();
        if (cmdArgs.Length >= 3 && cmdArgs[1] == "--elevated-inject")
        {
            Environment.Exit(GameLaunchService.RunElevatedInjection(cmdArgs[2]));
            return;
        }
        _window = new MainWindow();
        _window.Activate();
    }
}
