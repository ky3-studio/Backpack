using System.Collections.Concurrent;
using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Services;

internal static partial class GfxLoader
{
    private static readonly BitmapImage _placeholder = new(
        new Uri("ms-appx:///Assets/Quality/UI_ItemIcon_None.png"));

    private static readonly ConcurrentDictionary<string, string>        _cache    = new();
    private static readonly ConcurrentDictionary<string, Task<string?>> _inflight = new();

    private static readonly string _cacheDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "BackpackViewer", "icons");

    private static void Log(string msg) =>
        File.AppendAllText(
            Path.Combine(AppContext.BaseDirectory, "gfx_err.txt"),
            $"{DateTime.Now:HH:mm:ss.fff} {msg}\n");

    internal static Task WarmupAsync()
    {
        if (!Directory.Exists(_cacheDir)) return Task.CompletedTask;

        foreach (var path in Directory.EnumerateFiles(_cacheDir, "*.png"))
        {
            if (!IsValid(path))      { RemoveIfExists(path); continue; }
            if (!HasSrgbChunk(path)) { RemoveIfExists(path); continue; }
            _cache.TryAdd(Path.GetFileName(path), path);
        }

        return Task.CompletedTask;
    }

    internal static void BeginLoad(Uri uri, IIconUpdatable target)
    {
        var key  = HashedName(uri.OriginalString);
        var disk = Path.Combine(_cacheDir, key);

        if (_cache.TryGetValue(key, out var cached)) { Attach(target, cached); return; }

        if (IsValid(disk))
        {
            if (!HasSrgbChunk(disk))
                RemoveIfExists(disk);
            else
            {
                _cache[key] = disk;
                Attach(target, disk);
                return;
            }
        }

        target.IconSource = _placeholder;
        _ = LoadAndSetAsync(uri, key, disk, target);
    }

    private static void Attach(IIconUpdatable target, string diskPath)
    {
        var bmp = new BitmapImage();
        bmp.UriSource     = new Uri(diskPath);
        target.IconSource = bmp;
    }

    private static async Task LoadAndSetAsync(Uri uri, string key, string disk, IIconUpdatable target)
    {
        var result = await _inflight.GetOrAdd(key, _ => DownloadWithRetryAsync(uri, disk, key));
        _inflight.TryRemove(key, out _);

        if (result is null) return;

        _cache[key] = disk;
        Attach(target, disk);
    }

    private static void RemoveIfExists(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
