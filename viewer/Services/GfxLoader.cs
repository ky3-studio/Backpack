using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Backpack.Viewer.Services;

internal static class GfxLoader
{
    private static readonly HttpClient _client = new(
        new HttpClientHandler
        {
            AutomaticDecompression  = DecompressionMethods.None,
            MaxConnectionsPerServer = 32,
        });

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

    private static string HashedName(string url)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToHexStringLower(hash) + ".png";
    }

    private static bool IsValid(string path) =>
        File.Exists(path) && new FileInfo(path).Length > 0;

    private static bool HasSrgbChunk(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            Span<byte> header = stackalloc byte[64];
            int read = fs.Read(header);
            for (int i = 8; i < read - 4; i++)
            {
                if (header[i] == 0x73 && header[i + 1] == 0x52 &&
                    header[i + 2] == 0x47 && header[i + 3] == 0x42)
                    return true;
                if (header[i] == 0x49 && header[i + 1] == 0x44 &&
                    header[i + 2] == 0x41 && header[i + 3] == 0x54)
                    return false;
            }
            return false;
        }
        catch { return true; }
    }

    private static uint PngCrc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }
        return ~crc;
    }

    private static void InjectSrgbChunk(string path)
    {
        const int insertAt = 33;

        var data = File.ReadAllBytes(path);
        if (data.Length < insertAt) return;

        ReadOnlySpan<byte> typeAndData = [0x73, 0x52, 0x47, 0x42, 0x00];
        var crcVal = PngCrc32(typeAndData);

        byte[] chunk =
        [
            0x00, 0x00, 0x00, 0x01,
            0x73, 0x52, 0x47, 0x42,
            0x00,
            (byte)(crcVal >> 24), (byte)(crcVal >> 16),
            (byte)(crcVal >> 8),  (byte)crcVal,
        ];

        var result = new byte[data.Length + chunk.Length];
        Buffer.BlockCopy(data,  0,         result, 0,                  insertAt);
        Buffer.BlockCopy(chunk, 0,         result, insertAt,           chunk.Length);
        Buffer.BlockCopy(data,  insertAt,  result, insertAt + chunk.Length, data.Length - insertAt);

        File.WriteAllBytes(path, result);
    }

    internal static Task WarmupAsync()
    {
        if (!Directory.Exists(_cacheDir)) return Task.CompletedTask;

        foreach (var path in Directory.EnumerateFiles(_cacheDir, "*.png"))
        {
            if (!IsValid(path)) continue;
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
            {
                RemoveIfExists(disk);
            }
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

    private static async Task<string?> DownloadWithRetryAsync(Uri uri, string disk, string key)
    {
        const int maxRetries = 2;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await DownloadFileAsync(uri, disk).ConfigureAwait(false);
                InjectSrgbChunk(disk);
                return disk;
            }
            catch when (attempt < maxRetries)
            {
                RemoveIfExists(disk);
                await Task.Delay(2000).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                RemoveIfExists(disk);
                Log($"NET {ex.GetType().Name}: {ex.Message} | {key}");
                return null;
            }
        }

        return null;
    }

    private static async Task DownloadFileAsync(Uri uri, string disk)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(disk)!);

        using var resp = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        using var httpStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var fileStream = File.Create(disk);
        await httpStream.CopyToAsync(fileStream).ConfigureAwait(false);
    }

    private static void RemoveIfExists(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
