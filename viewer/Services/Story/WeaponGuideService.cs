using System.Net.Http;
using System.Text.Json;

namespace Backpack.Viewer.Services.Story;

internal sealed class WeaponGuideService
{
    private readonly HttpClient _http;

    public WeaponGuideService(HttpClient http)
    {
        _http = http;
    }

    internal sealed record AvatarGuide(string Icon, int Rank, string Name);

    internal sealed record GuideData(IReadOnlyList<AvatarGuide> Builds, IReadOnlyList<AvatarGuide> Abyss);

    public async Task<GuideData?> FetchGuidesAsync(uint weaponId)
    {
        try
        {
            var json = await _http.GetStringAsync(
                $"https://gi.yatta.moe/api/v2/static/advanced/weaponGuides/{weaponId}").ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("avatarList", out var list))
                return null;

            return new GuideData(
                CollectAvatars(data, "gwData", list),
                CollectAvatars(data, "azaData", list));
        }
        catch
        {
            return null;
        }
    }

    private static List<AvatarGuide> CollectAvatars(JsonElement data, string key, JsonElement list)
    {
        List<AvatarGuide> result = [];
        if (!data.TryGetProperty(key, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var idEl in arr.EnumerateArray())
        {
            if (idEl.GetString() is not { Length: > 0 } idStr) continue;
            if (!list.TryGetProperty(idStr, out var av)) continue;
            if (!av.TryGetProperty("icon", out var ic) || ic.GetString() is not { Length: > 0 } icon) continue;
            var rank = av.TryGetProperty("rank", out var rk) && rk.TryGetInt32(out var r) ? r : 5;
            var name = av.TryGetProperty("route", out var rt) ? rt.GetString() ?? string.Empty : string.Empty;
            result.Add(new AvatarGuide(icon, rank, name));
        }

        return result;
    }
}
