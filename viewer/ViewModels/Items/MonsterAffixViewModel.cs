using Backpack.Viewer;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels;

public sealed class MonsterAffixViewModel
{
    public MonsterAffixViewModel(string name, string description)
    {
        Name        = name;
        Description = description;
    }

    public string Name        { get; }
    public string Description  { get; }

    public Visibility NameVisibility        => (!string.IsNullOrEmpty(Name)).ToVisibility();
    public Visibility DescriptionVisibility => (!string.IsNullOrEmpty(Description)).ToVisibility();
}
