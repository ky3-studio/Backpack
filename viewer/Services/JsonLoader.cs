using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Backpack.Viewer.Services;

internal static class JsonLoader
{
    internal static T? Load<T>(string path, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize(File.ReadAllText(path), typeInfo);
        }
        catch { }
        return default;
    }
}
