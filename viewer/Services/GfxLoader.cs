using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Services;

internal static class GfxLoader
{
    private static readonly HttpClient _client = new(
        new HttpClientHandler { AutomaticDecompression = DecompressionMethods.None });

    private static readonly BitmapImage _placeholder = new(
        new Uri("ms-appx:///Assets/Quality/UI_ItemIcon_None.png"));

    private static readonly ConcurrentDictionary<string, BitmapImage> _cache    = new();
    private static readonly ConcurrentDictionary<string, Task<string?>> _inflight = new();

    private static readonly string _cacheDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "BackpackViewer", "icons");

    private static readonly SemaphoreSlim _netSem = new(8, 8);

    private static string Key(Uri uri)
    {
        var s = uri.Segments;
        return s.Length >= 2 ? s[^2].TrimEnd('/') + "/" + s[^1] : s[^1];
    }

    private static void Log(string msg) =>
        File.AppendAllText(
            Path.Combine(AppContext.BaseDirectory, "gfx_err.txt"),
            $"{DateTime.Now:HH:mm:ss.fff} {msg}\n");

    internal static Task WarmupAsync()
    {
        if (!Directory.Exists(_cacheDir)) return Task.CompletedTask;

        foreach (var path in Directory.EnumerateFiles(_cacheDir, "*.png", SearchOption.AllDirectories))
        {
            var dir  = Path.GetFileName(Path.GetDirectoryName(path)) ?? string.Empty;
            var file = Path.GetFileName(path);
            var key  = dir.Length > 0 && dir != "icons" ? dir + "/" + file : file;
            if (!_cache.ContainsKey(key))
                _cache[key] = new BitmapImage(new Uri(path));
        }

        return Task.CompletedTask;
    }

    internal static void BeginLoad(Uri uri, IIconUpdatable target)
    {
        var key = Key(uri);
        if (_cache.TryGetValue(key, out var hit)) { target.IconSource = hit; return; }
        target.IconSource = _placeholder;
        _ = LoadAndSetAsync(uri, key, target);
    }

    private static async Task LoadAndSetAsync(Uri uri, string key, IIconUpdatable target)
    {
        var disk = await _inflight.GetOrAdd(key, _ => DownloadAsync(uri, key));
        if (disk is null) return;

        if (_cache.TryGetValue(key, out var cached)) { target.IconSource = cached; return; }

        var bmp = new BitmapImage(new Uri(disk));
        _cache[key] = bmp;
        target.IconSource = bmp;
    }

    private static async Task<string?> DownloadAsync(Uri uri, string key)
    {
        try
        {
            var disk = Path.Combine(_cacheDir, key);
            if (File.Exists(disk)) return disk;

            await _netSem.WaitAsync().ConfigureAwait(false);
            try
            {
                using var resp = await _client.GetAsync(uri).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) { Log($"HTTP {(int)resp.StatusCode} {key}"); return null; }
                var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                Directory.CreateDirectory(Path.GetDirectoryName(disk)!);
                await File.WriteAllBytesAsync(disk, bytes).ConfigureAwait(false);
                return disk;
            }
            catch (Exception ex) { Log($"NET {ex.GetType().Name}: {ex.Message} | {key}"); return null; }
            finally { _netSem.Release(); }
        }
        finally
        {
            _inflight.TryRemove(key, out _);
        }
    }
}
