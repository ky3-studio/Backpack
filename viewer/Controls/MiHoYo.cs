using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Backpack.Viewer.Controls;

public static class MiHoYo
{
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.RegisterAttached("Description", typeof(string), typeof(MiHoYo),
            new PropertyMetadata(null, OnDescriptionChanged));

    public static void SetDescription(RichTextBlock element, string? value)
        => element.SetValue(DescriptionProperty, value);

    public static string? GetDescription(RichTextBlock element)
        => (string?)element.GetValue(DescriptionProperty);

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RichTextBlock rtb) return;
        rtb.Blocks.Clear();
        var text = e.NewValue as string ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;
        var para = new Paragraph();
        Render(para.Inlines, text);
        rtb.Blocks.Add(para);
    }

    private static readonly Regex _pattern = new(
        @"<color=#(?<hex>[0-9A-Fa-f]{8})>|</color>|<i>|</i>|\{LINK#[^}]+\}|\{/LINK\}|\n",
        RegexOptions.Compiled);

    private static void Render(InlineCollection inlines, string text)
    {
        int     pos     = 0;
        Brush?  color   = null;
        bool    italic  = false;

        foreach (Match m in _pattern.Matches(text))
        {
            if (m.Index > pos)
                Emit(inlines, text[pos..m.Index], color, italic);

            pos = m.Index + m.Length;

            if (m.Value.StartsWith("<color="))
                color = new SolidColorBrush(ParseHex(m.Groups["hex"].Value));
            else if (m.Value == "</color>")
                color = null;
            else if (m.Value == "<i>")
                italic = true;
            else if (m.Value == "</i>")
                italic = false;
            else if (m.Value == "\n")
                inlines.Add(new LineBreak());
        }

        if (pos < text.Length)
            Emit(inlines, text[pos..], color, italic);
    }

    private static void Emit(InlineCollection inlines, string text, Brush? color, bool italic)
    {
        if (string.IsNullOrEmpty(text)) return;
        var run = new Run { Text = text };
        if (color is not null)  run.Foreground = color;
        if (italic)             run.FontStyle  = Windows.UI.Text.FontStyle.Italic;
        inlines.Add(run);
    }

    private static Color ParseHex(string hex8)
        => Color.FromArgb(
            Convert.ToByte(hex8[6..], 16),
            Convert.ToByte(hex8[..2], 16),
            Convert.ToByte(hex8[2..4], 16),
            Convert.ToByte(hex8[4..6], 16));
}
