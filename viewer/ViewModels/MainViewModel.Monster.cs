using System.Collections.ObjectModel;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<MonsterViewModel> Monsters { get; } = [];

    internal void RebuildMonsters()
    {
        Monsters.Clear();
        foreach (var m in _monsterMeta.GetDefaultEntries())
            Monsters.Add(new MonsterViewModel(m, _monsterMeta));
    }
}
