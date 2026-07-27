using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using Backpack.Viewer;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Models;
using Backpack.Viewer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Backpack.Viewer.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly DispatcherQueue      _dispatcher;
    private readonly MaterialMetaService  _materialMeta;
    private readonly FoodMetaService      _foodMeta;
    private readonly WeaponMetaService    _weaponMeta;
    private readonly ArtifactMetaService  _artifactMeta;
    private readonly BackpackDbService    _db;

    public GamePathService GamePathService { get; }

    public ObservableCollection<WeaponViewModel>       Weapons        { get; } = [];
    public ObservableCollection<ArtifactViewModel>     Artifacts      { get; } = [];
    public ObservableCollection<MaterialGroupViewModel> MaterialGroups { get; } = [];
    public ObservableCollection<FoodGroupViewModel>    FoodGroups     { get; } = [];

    private readonly Dictionary<uint, ulong>      _activeCounts = [];
    private const string PropKeyResin = "原粹树脂";
    private static readonly Dictionary<string, uint> PropIdMap = new()
    {
        ["原石"]    = 201,
        ["摩拉"]    = 202,
        ["创世结晶"] = 203,
        ["洞天宝钱"] = 204,
        [PropKeyResin] = 106,
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataVisibility))]
    [NotifyPropertyChangedFor(nameof(SetupVisibility))]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisibility))]
    [NotifyPropertyChangedFor(nameof(LaunchButtonVisibility))]
    [NotifyPropertyChangedFor(nameof(CanLaunch))]
    public partial bool HasSelectedPath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisibility))]
    [NotifyPropertyChangedFor(nameof(CanLaunch))]
    public partial bool IsLaunching { get; set; } = false;

    [ObservableProperty]
    public partial bool IsGameRunning { get; set; } = false;

    [ObservableProperty]
    public partial string StatusText { get; set; } = Localized.Get("StatusWaiting");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SetupErrorVisibility))]
    public partial string SetupError { get; set; } = string.Empty;

    public Visibility DataVisibility         => HasSelectedPath.ToVisibility();
    public Visibility SetupVisibility        => HasSelectedPath.ToCollapsed();
    public Visibility ProgressRingVisibility => (HasSelectedPath && IsLaunching).ToVisibility();
    public Visibility PathListVisibility     => (GamePathService.Paths.Count > 0).ToVisibility();
    public Visibility SetupErrorVisibility   => (!string.IsNullOrEmpty(SetupError)).ToVisibility();
    public Visibility LaunchButtonVisibility => HasSelectedPath.ToVisibility();
    public bool       CanLaunch              => HasSelectedPath && !IsLaunching;

    public MainViewModel(DispatcherQueue dispatcher, GamePathService gamePathService,
        MaterialMetaService materialMeta, FoodMetaService foodMeta, WeaponMetaService weaponMeta,
        ArtifactMetaService artifactMeta, BackpackDbService db)
    {
        _dispatcher   = dispatcher;
        _materialMeta = materialMeta;
        _foodMeta     = foodMeta;
        _weaponMeta   = weaponMeta;
        _artifactMeta = artifactMeta;
        _db           = db;
        GamePathService = gamePathService;
        HasSelectedPath = gamePathService.HasSelection;
        gamePathService.Paths.CollectionChanged += (_, _) => OnPropertyChanged(nameof(PathListVisibility));

        var dbWeapons = db.LoadWeapons();
        if (dbWeapons.Count > 0)
            foreach (var e in dbWeapons) Weapons.Add(new WeaponViewModel(e, _weaponMeta));
        else
            LoadDefaultWeapons();

        var dbArtifacts = db.LoadArtifacts();
        if (dbArtifacts.Count > 0)
            foreach (var e in dbArtifacts) Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
        else
            LoadDefaultArtifacts();

        _activeCounts = db.LoadMaterialCounts();
        RebuildMaterialGroups();
        RebuildFoodGroups();
    }

    private void LoadDefaultArtifacts()
    {
        foreach (var e in _artifactMeta.GetDefaultEntries())
            Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
    }

    private void LoadDefaultWeapons()
    {
        foreach (var e in _weaponMeta.GetDefaultEntries())
            Weapons.Add(new WeaponViewModel(e, _weaponMeta));
    }

    private void RebuildMaterialGroups()
    {
        MaterialGroups.Clear();
        foreach (var (label, ids) in _materialMeta.Groups)
        {
            MaterialGroups.Add(new MaterialGroupViewModel(
                label,
                [.. ids.Select(id =>
                {
                    var count = _activeCounts.TryGetValue(id, out var c) ? c : 0UL;
                    var entry = new MaterialEntry(id, _materialMeta.GetName(id), string.Empty, count);
                    return new MaterialViewModel(entry, _materialMeta);
                })]));
        }
    }

    private void RebuildFoodGroups()
    {
        FoodGroups.Clear();
        foreach (var (label, ids) in _foodMeta.Groups)
        {
            FoodGroups.Add(new FoodGroupViewModel(
                label,
                [.. ids
                    .Select(id => (id, meta: _foodMeta.GetMeta(id)))
                    .Where(x => x.meta is not null)
                    .Select(x =>
                    {
                        var count = _activeCounts.TryGetValue(x.id, out var c) ? c : 0UL;
                        var ingredients = x.meta!.Ingredients
                            .Select(ing =>
                            {
                                var held    = _activeCounts.TryGetValue(ing.Id, out var h) ? h : 0UL;
                                var iconUri = _materialMeta.GetMeta(ing.Id).IconUri;
                                return new IngredientViewModel(ing, held, iconUri);
                            })
                            .ToList();
                        return new FoodViewModel(x.meta!, count, ingredients);
                    })]));
        }
    }

    public event Action? DataReceived;

    public void OnPacketReceived(object? _, (string Event, string Json) args)
    {
        var (evt, json) = args;
        _dispatcher.TryEnqueue(() => Apply(evt, json));
    }

    private void Apply(string evt, string json)
    {
        switch (evt)
        {
            case "weapon":
            {
                var bag = JsonSerializer.Deserialize<WeaponBag>(json);
                if (bag is null) return;
                Weapons.Clear();
                foreach (var e in bag.Weapons) Weapons.Add(new WeaponViewModel(e, _weaponMeta));
                _db.SaveWeapons(bag.Weapons);
                break;
            }
            case "artifact":
            {
                var bag = JsonSerializer.Deserialize<ArtifactBag>(json);
                if (bag is null) return;
                Artifacts.Clear();
                foreach (var e in bag.Artifacts) Artifacts.Add(new ArtifactViewModel(e, _artifactMeta));
                _db.SaveArtifacts(bag.Artifacts);
                break;
            }
            case "material":
            {
                var bag = JsonSerializer.Deserialize<MaterialBag>(json);
                if (bag is null) return;
                foreach (var e in bag.Materials)
                    _activeCounts[e.Id] = e.Count;
                _db.SaveMaterials(_activeCounts);
                RebuildMaterialGroups();
                RebuildFoodGroups();
                break;
            }
            case "prop":
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
                if (dict is null) return;
                foreach (var (key, val) in dict)
                {
                    if (!PropIdMap.TryGetValue(key, out var id)) continue;
                    _activeCounts[id] = key == PropKeyResin ? (ulong)(val / 100) : (ulong)val;
                }
                _db.SaveMaterials(_activeCounts);
                RebuildMaterialGroups();
                RebuildFoodGroups();
                break;
            }
        }
        IsLaunching = false;
        StatusText = $"{Localized.Get("StatusReceived")} · {DateTime.Now:HH:mm:ss}";
        DataReceived?.Invoke();
    }
}
