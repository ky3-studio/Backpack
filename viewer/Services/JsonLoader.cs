using System.IO;
using System.Text.Json;

namespace Backpack.Viewer.Services;

internal static class JsonLoader
{
    private static readonly JsonSerializerOptions _opts =
        new() { PropertyNameCaseInsensitive = true };

    internal static T? Load<T>(string path)
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path), _opts);
        }
        catch { }
        return default;
    }
}
