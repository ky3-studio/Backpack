using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed class GamePathService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BackpackViewer",
        "settings.json");

    private const string CnExeName = "YuanShen.exe";

    public ObservableCollection<string> Paths { get; }

    public string? SelectedPath { get; private set; }

    public bool HasSelection => SelectedPath is not null;

    public GamePathService()
    {
        var s = Load();
        Paths = new ObservableCollection<string>(
            s.GamePaths.Where(p => !string.IsNullOrWhiteSpace(p)));
        SelectedPath = string.IsNullOrWhiteSpace(s.SelectedPath) ? null : s.SelectedPath;
    }

    public bool TryAdd(string path)
    {
        if (!path.EndsWith(CnExeName, StringComparison.OrdinalIgnoreCase))
            return false;
        if (Paths.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
            return false;
        Paths.Add(path);
        Save();
        return true;
    }

    public void Remove(string path)
    {
        Paths.Remove(path);
        if (string.Equals(SelectedPath, path, StringComparison.OrdinalIgnoreCase))
            SelectedPath = null;
        Save();
    }

    public void Select(string path)
    {
        SelectedPath = path;
        Save();
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new SettingsData
        {
            GamePaths    = [.. Paths],
            SelectedPath = SelectedPath ?? string.Empty,
        }));
    }

    private static SettingsData Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SettingsPath)) ?? new();
        }
        catch { }
        return new();
    }

    private sealed class SettingsData
    {
        [JsonPropertyName("gamePaths")]
        public List<string> GamePaths { get; init; } = [];

        [JsonPropertyName("selectedPath")]
        public string SelectedPath { get; init; } = string.Empty;
    }
}
