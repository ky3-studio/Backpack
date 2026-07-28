using System.IO;
using System.Net;
using System.Net.Http;

namespace Backpack.Viewer.Services;

internal static partial class GfxLoader
{
    private static readonly HttpClient _client = new(
        new HttpClientHandler
        {
            AutomaticDecompression  = DecompressionMethods.None,
            MaxConnectionsPerServer = 32,
        });

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
}
