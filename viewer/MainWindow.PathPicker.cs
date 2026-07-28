using Backpack.Viewer.Localization;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;

namespace Backpack.Viewer;

public sealed partial class MainWindow
{
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
}
