using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backpack.Viewer.Services;

public sealed class FoodMetaService
{
    private readonly Dictionary<uint, FoodMeta> _map;

    public FoodMetaService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Food", "foods.json");
        try
        {
            if (File.Exists(path))
            {
                var items = JsonSerializer.Deserialize<RawEntry[]>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                _map = items.DistinctBy(e => e.Id).ToDictionary(
                    e => (uint)e.Id,
                    e => new FoodMeta(
                        e.Name, e.Type, e.Variant, e.Rank, e.Icon,
                        e.Character ?? string.Empty,
                        ParseIngredients(e.IngredientsRaw)));
                return;
            }
        }
        catch { }
        _map = [];
    }

    private static IReadOnlyList<IngredientMeta> ParseIngredients(JsonElement raw)
    {
        static IngredientMeta ParseOne(JsonElement e) => new(
            (uint)e.GetProperty("id").GetInt32(),
            e.GetProperty("name").GetString() ?? string.Empty,
            e.GetProperty("amount").GetInt32());

        return raw.ValueKind == JsonValueKind.Array
            ? [.. raw.EnumerateArray().Select(ParseOne)]
            : [ParseOne(raw)];
    }

    public FoodMeta? GetMeta(uint id) => _map.GetValueOrDefault(id);

    public IReadOnlyCollection<uint> AllIds => _map.Keys;

    private static readonly string[] _groupOrder = ["攻击类料理", "防御类料理", "恢复类料理", "冒险类料理"];

    public static int DishTypeOrder(string dishType)
    {
        var idx = Array.IndexOf(_groupOrder, dishType);
        return idx >= 0 ? idx : 99;
    }

    public sealed record FoodMeta(
        string                        Name,
        string                        DishType,
        string                        Variant,
        int                           Rank,
        string                        Icon,
        string                        Character,
        IReadOnlyList<IngredientMeta> Ingredients
    );

    public sealed record IngredientMeta(uint Id, string Name, int Amount);

    private sealed class RawEntry
    {
        [JsonPropertyName("id")]          public int         Id             { get; set; }
        [JsonPropertyName("name")]        public string      Name           { get; set; } = string.Empty;
        [JsonPropertyName("type")]        public string      Type           { get; set; } = string.Empty;
        [JsonPropertyName("variant")]     public string      Variant        { get; set; } = string.Empty;
        [JsonPropertyName("rank")]        public int         Rank           { get; set; }
        [JsonPropertyName("icon")]        public string      Icon           { get; set; } = string.Empty;
        [JsonPropertyName("recipe")]      public string      Recipe         { get; set; } = string.Empty;
        [JsonPropertyName("character")]   public string?     Character      { get; set; }
        [JsonPropertyName("ingredients")] public JsonElement IngredientsRaw { get; set; }
    }
}
