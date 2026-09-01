using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.ViewModels.Search;
using Backpack.Viewer.Views.Controls;
using Backpack.Viewer.Views.Controls.AutoSuggestBox;
using Backpack.Viewer.Views.Helpers;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace Backpack.Viewer.Views;

[DependencyProperty<ObservableCollection<GroupViewModel<MaterialViewModel>>>("MaterialGroups", PropertyChangedCallbackName = "OnMaterialGroupsChanged")]
public sealed partial class MaterialPage : UserControl, IDisposable
{

    internal IReadOnlyDictionary<string, SearchToken>? AvailableTokens { get; private set; }
    internal ObservableCollection<SearchToken> FilterTokens { get; } = [];
    public string? FilterText { get; set; }
    public ICommand ApplyFilterCommand { get; }

    public IReadOnlyList<MaterialViewModel>? CurrentItems =>
        SearchTokenFilter.Apply(
            (_controller.SelectedGroup as GroupViewModel<MaterialViewModel>)?.Items,
            FilterTokens,
            ItemSearchTokens.MatchValue);

    private readonly TabbedGroupController<GroupViewModel<MaterialViewModel>> _controller;
    private PooledElementFactory _listFactory = null!;
    private PooledElementFactory _gridFactory = null!;

    public MaterialPage()
    {
        InitializeComponent();
        ApplyFilterCommand = new RelayCommand(() => Bindings.Update());
        _controller = new TabbedGroupController<GroupViewModel<MaterialViewModel>>(TabPivot, OnGroupChanged);
        SetupTemplate();
        ItemsPanelSelector.RegisterPropertyChangedCallback(LayoutSwitch.CurrentProperty, OnLayoutChanged);
    }

    private void SetupTemplate()
    {
        _listFactory = new PooledElementFactory((DataTemplate)Resources["SimpleCardTemplate"]);
        _gridFactory = new PooledElementFactory((DataTemplate)Resources["SimpleGridTemplate"]);
        CardRepeater.ItemTemplate = _listFactory;
    }

    private void OnLayoutChanged(DependencyObject sender, DependencyProperty dp)
    {
        bool grid = ItemsPanelSelector.Current == LayoutSwitch.Grid;
        CardRepeater.Layout = new UniformGridLayout
        {
            MinItemWidth     = grid ? 96 : 260,
            MinColumnSpacing = 8,
            MinRowSpacing    = 8,
            ItemsStretch     = grid ? UniformGridLayoutItemsStretch.None : UniformGridLayoutItemsStretch.Fill,
        };
        CardRepeater.ItemTemplate = grid ? _gridFactory : _listFactory;
    }

    private void OnGroupChanged()
    {
        var items = (_controller.SelectedGroup as GroupViewModel<MaterialViewModel>)?.Items ?? [];
        AvailableTokens = ItemSearchTokens.Build(items);
        FilterTokens.Clear();
        Bindings.Update();
    }

    private static void OnMaterialGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MaterialPage page)
            page._controller.Bind((ObservableCollection<GroupViewModel<MaterialViewModel>>?)e.NewValue);
    }

    private void OnTabChanged(object sender, SelectionChangedEventArgs e) =>
        _controller.OnTabSelectionChanged(e);

    private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FrameworkElement fe)
            fe.DataContext = sender.ItemsSourceView?.GetAt(args.Index);
    }

    private void OnElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is FrameworkElement fe)
            fe.DataContext = null;
    }

    public async void ExportMaterials()
    {
        var materialGroups = MaterialGroups;
        if (materialGroups is null || materialGroups.Count == 0)
            return;

        var hwnd = WindowNative.GetWindowHandle(App.AppWindow);
        var path = Win32FilePicker.SaveFile(
            hwnd,
            SR.MaterialExportDialogTitle,
            "materials.csv",
            "csv",
            SR.MaterialExportFilterName,
            "*.csv");
        if (string.IsNullOrEmpty(path))
            return;
        if (!path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            path += ".csv";

        var sb = new StringBuilder();
        ReadOnlySpan<string?> header =
            [Csv(SR.MaterialExportColCategory), Csv(SR.MaterialExportColName), Csv(SR.MaterialExportColCount)];
        sb.AppendLine(string.Join(',', header));
        foreach (var group in materialGroups)
        {
            foreach (var item in group.Items)
            {
                ReadOnlySpan<string?> row = [Csv(group.Header), Csv(item.Name), Csv(item.RawCount.ToString())];
                sb.AppendLine(string.Join(',', row));
            }
        }

        try
        {
            await File.WriteAllTextAsync(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            await ShowResultDialog(SR.MaterialExportSuccessTitle, string.Format(SR.MaterialExportSuccessFmt, path));
        }
        catch (Exception ex)
        {
            await ShowResultDialog(SR.MaterialExportFailedTitle, ex.Message);
        }

        static string Csv(string value)
        {
            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
                return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }

    private async Task ShowResultDialog(string title, string message)
    {
        if (XamlRoot is null)
            return;
        await new ContentDialog
        {
            XamlRoot        = XamlRoot,
            Title           = title,
            Content         = message,
            CloseButtonText = SR.CommonOk,
        }.ShowAsync();
    }

    public void Dispose() => _controller.Dispose();
}
