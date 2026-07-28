using System.Collections.ObjectModel;
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
    private readonly GadgetMetaService    _gadgetMeta;
    private readonly AssetMetaService     _assetMeta;
    private readonly AvatarMetaService    _avatarMeta;
    private readonly AvatarDetailService  _avatarDetail;
    private readonly BackpackDbService    _db;

    public GamePathService GamePathService { get; }

    public ObservableCollection<WeaponViewModel>        Weapons        { get; } = [];
    public ObservableCollection<ArtifactViewModel>      Artifacts      { get; } = [];
    public ObservableCollection<AvatarViewModel>        Avatars        { get; } = [];
    public ObservableCollection<GroupViewModel<MaterialViewModel>>  MaterialGroups { get; } = [];
    public ObservableCollection<GroupViewModel<FoodViewModel>>      FoodGroups     { get; } = [];
    public ObservableCollection<GroupViewModel<GadgetViewModel>>    GadgetGroups   { get; } = [];
    public ObservableCollection<GroupViewModel<AssetViewModel>>     AssetGroups    { get; } = [];

    private readonly Dictionary<uint, ulong> _activeCounts = [];
    private readonly Dictionary<uint, long>  _activeProps  = [];

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
        ArtifactMetaService artifactMeta, GadgetMetaService gadgetMeta, AssetMetaService assetMeta,
        AvatarMetaService avatarMeta, AvatarDetailService avatarDetail, BackpackDbService db)
    {
        _dispatcher   = dispatcher;
        _materialMeta = materialMeta;
        _foodMeta     = foodMeta;
        _weaponMeta   = weaponMeta;
        _artifactMeta = artifactMeta;
        _gadgetMeta   = gadgetMeta;
        _assetMeta    = assetMeta;
        _avatarMeta   = avatarMeta;
        _avatarDetail = avatarDetail;
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

        RebuildAvatars(db.LoadAvatars());

        _activeCounts = db.LoadMaterialCounts();
        _activeProps  = db.LoadProps();
        RebuildMaterialGroups();
        RebuildFoodGroups();
        RebuildGadgetGroups();
        RebuildAssetGroups();
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

    internal void RebuildAvatars(IReadOnlyList<AvatarEntry> realData)
    {
        var map = new Dictionary<uint, AvatarEntry>();
        foreach (var e in realData) map[e.Id] = e;

        bool hasData = map.Count > 0;

        Avatars.Clear();
        foreach (var m in _avatarMeta.GetDefaultEntries())
        {
         
            if (hasData && !map.ContainsKey(m.Id)) continue;
            var entry = map.TryGetValue(m.Id, out var real)
                ? real
                : new AvatarEntry(m.Id, string.Empty, 0, 0, 0, [], [], [], []);
            Avatars.Add(ToAvatarViewModel(entry));
        }
    }

    private AvatarViewModel ToAvatarViewModel(AvatarEntry e)
    {
       
        var weaponSet = new HashSet<string>(Weapons.Select(w => w.Source.Guid));
        var wGuid     = e.Equips.FirstOrDefault(g => weaponSet.Contains(g)) ?? string.Empty;
        var weapon = !string.IsNullOrEmpty(wGuid)
            ? Weapons.FirstOrDefault(w => w.Source.Guid == wGuid)
            : null;
        return new AvatarViewModel(e, _avatarMeta, _avatarDetail, weapon);
    }
}
