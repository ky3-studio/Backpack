using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;

namespace Backpack.Viewer;

public sealed partial class MainWindow
{
    private void PickGamePath()
    {
        ViewModel.SetupError = string.Empty;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var path = Win32FilePicker.PickFile(hwnd, (SR.PathPickerFilterName, "*.exe"));
        if (string.IsNullOrEmpty(path)) return;
        if (!ViewModel.GamePathService.TryAdd(path))
            ViewModel.SetupError = SR.PathInvalidMsg;
    }
}
