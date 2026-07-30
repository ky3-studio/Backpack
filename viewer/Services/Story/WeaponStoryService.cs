using System.Net.Http;
using System.Text.Json;

namespace Backpack.Viewer.Services.Story;

internal sealed class WeaponStoryService
{
    private static readonly HttpClient _http = new();

    public async Task<string?> FetchStoryAsync(uint weaponId)
    {
        try
        {
            var json = await _http.GetStringAsync(
                $"https://gi.yatta.moe/api/v2/chs/readable/Weapon{weaponId}").ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data))
                return null;

            var text = data.GetString()?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }
}
