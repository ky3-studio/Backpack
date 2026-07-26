using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace Backpack.Viewer.Services;

internal static class GfxLoader
{
    private static readonly HttpClient _client = new(
        new HttpClientHandler { AutomaticDecompression = DecompressionMethods.None });

    private static readonly BitmapImage _placeholder = new(
        new Uri("ms-appx:///Assets/Quality/UI_ItemIcon_None.png"));

    internal static async Task<BitmapImage?> TryLoadAsync(Uri uri)
    {
        try
        {
            using var resp = await _client.GetAsync(uri);
            if (!resp.IsSuccessStatusCode) return null;
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            var bmp   = new BitmapImage();
            using var ms = new InMemoryRandomAccessStream();
            await ms.WriteAsync(bytes.AsBuffer());
            ms.Seek(0);
            await bmp.SetSourceAsync(ms);
            return bmp;
        }
        catch { return null; }
    }

    internal static async Task HandleIconFailedAsync(object sender)
    {
        if (sender is not Image img) return;
        if (img.Source is BitmapImage bi && bi.UriSource is { } uri)
        {
            var bmp = await TryLoadAsync(uri);
            if (bmp is not null)
            {
                var vm = FindIconViewModel(img);
                if (vm is not null)
                    vm.IconSource = bmp;
                else
                    img.Source = bmp;
                return;
            }
        }
        img.Source = _placeholder;
    }

    private static IIconUpdatable? FindIconViewModel(DependencyObject element)
    {
        var current = VisualTreeHelper.GetParent(element);
        while (current is not null)
        {
            if (current is FrameworkElement fe && fe.Tag is IIconUpdatable vm)
                return vm;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
