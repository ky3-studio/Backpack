using Backpack.Viewer;
using Backpack.Viewer.Localization;
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
    private readonly MonsterMetaService   _monsterMeta;
    private readonly BackpackDbService    _db;

    public GamePathService GamePathService { get; }

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
    public partial string StatusText { get; set; } = SR.StatusWaiting;

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

    public MainViewModel(GamePathService gamePathService,
        MaterialMetaService materialMeta, FoodMetaService foodMeta, WeaponMetaService weaponMeta,
        ArtifactMetaService artifactMeta, GadgetMetaService gadgetMeta, AssetMetaService assetMeta,
        AvatarMetaService avatarMeta, AvatarDetailService avatarDetail, MonsterMetaService monsterMeta, BackpackDbService db)
    {
        _dispatcher   = DispatcherQueue.GetForCurrentThread();
        _materialMeta = materialMeta;
        _foodMeta     = foodMeta;
        _weaponMeta   = weaponMeta;
        _artifactMeta = artifactMeta;
        _gadgetMeta   = gadgetMeta;
        _assetMeta    = assetMeta;
        _avatarMeta   = avatarMeta;
        _avatarDetail = avatarDetail;
        _monsterMeta  = monsterMeta;
        _db           = db;
        GamePathService = gamePathService;
        HasSelectedPath = gamePathService.HasSelection;
        gamePathService.Paths.CollectionChanged += (_, _) => OnPropertyChanged(nameof(PathListVisibility));

        var dbWeapons = db.LoadWeapons();
        if (dbWeapons.Count > 0)
            foreach (var e in dbWeapons) Weapons.Add(new WeaponViewModel(e, _weaponMeta, _materialMeta));
        else
            LoadDefaultWeapons();
        RebuildWeaponGroups();

        BuildArtifacts(db.LoadArtifacts());

        RebuildAvatars(db.LoadAvatars());

        _activeCounts = db.LoadMaterialCounts();
        _activeProps  = db.LoadProps();
        RebuildMaterialGroups();
        RebuildFoodGroups();
        RebuildGadgetGroups();
        RebuildAssetGroups();
        RebuildMonsters();
    }
}
