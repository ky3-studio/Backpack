using System.ComponentModel;

namespace Backpack.Viewer.ViewModels;

public sealed class FilterOption : INotifyPropertyChanged
{
    public string             Label      { get; set; } = string.Empty;
    public int                Rank       { get; set; }
    public IReadOnlyList<int> StarItems  { get; set; } = [];

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static FilterOption ForRank(int rank) => new()
    {
        Rank      = rank,
        Label     = $"{rank}",
        StarItems = [.. Enumerable.Range(0, rank)],
    };

    public static FilterOption ForType(string typeName) => new()
    {
        Label     = typeName,
        StarItems = [],
    };
}

public sealed class SearchSuggestion
{
    public string DisplayLabel { get; set; } = string.Empty;
    public string TextValue    { get; set; } = string.Empty;

    public static SearchSuggestion ForName(string name) => new()
    {
        DisplayLabel = name,
        TextValue    = name,
    };
}
