using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Backpack.Viewer.Localization;
using Backpack.Viewer.Services;
using Backpack.Viewer.ViewModels;
using Backpack.Viewer.Views.Controls;
using Backpack.Viewer.Views.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace Backpack.Viewer.Views;

public sealed partial class SaintCardPage : UserControl
{
    internal ObservableCollection<SaintCardViewModel> Cards { get; } = [];

    private PooledElementFactory _listFactory = null!;
    private PooledElementFactory _gridFactory = null!;

    public SaintCardPage()
    {
        InitializeComponent();
        SetupTemplate();
        ItemsPanelSelector.RegisterPropertyChangedCallback(LayoutSwitch.CurrentProperty, OnLayoutChanged);
        LoadCards();
    }

    private void SetupTemplate()
    {
        _listFactory = new PooledElementFactory((DataTemplate)Resources["SimpleCardTemplate"]);
        _gridFactory = new PooledElementFactory((DataTemplate)Resources["SimpleGridTemplate"]);
        CardRepeater.ItemTemplate = _listFactory;
    }

    private void LoadCards()
    {
        var path = Path.Combine(StaticResources.MetadataDir, "SaintCard", "saint_cards.json");
        var raw  = JsonLoader.Load(path, SaintCardCtx.Default.RawEntryArray) ?? [];
        Cards.Clear();
        foreach (var e in raw)
            Cards.Add(new SaintCardViewModel((uint)e.Id, e.Name, 0UL, e.Rank, e.Icon));
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

    public async void Export()
    {
        if (Cards.Count == 0)
            return;

        var hwnd = WindowNative.GetWindowHandle(App.AppWindow);
        var path = Win32FilePicker.SaveFile(
            hwnd,
            SR.SaintCardExportDialogTitle,
            "saint_cards.csv",
            "csv",
            SR.MaterialExportFilterName,
            "*.csv");
        if (string.IsNullOrEmpty(path))
            return;
        if (!path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            path += ".csv";

        var sb = new StringBuilder();
        ReadOnlySpan<string?> header =
            [Csv(SR.MaterialExportColName), Csv(SR.MaterialExportColCount)];
        sb.AppendLine(string.Join(',', header));
        foreach (var card in Cards)
        {
            ReadOnlySpan<string?> row = [Csv(card.Name), Csv(card.RawCount.ToString())];
            sb.AppendLine(string.Join(',', row));
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

    [JsonSerializable(typeof(RawEntry[]))]
    private partial class SaintCardCtx : JsonSerializerContext { }

    private sealed class RawEntry
    {
        [JsonPropertyName("id")]   public int    Id   { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("rank")] public int    Rank { get; set; }
        [JsonPropertyName("icon")] public string Icon { get; set; } = string.Empty;
    }
}
