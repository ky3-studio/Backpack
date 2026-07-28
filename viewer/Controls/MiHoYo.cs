using Backpack.Viewer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

namespace Backpack.Viewer.Controls;

public static class MiHoYo
{
    private static HyperLinkService? _hyperLinkSvc;
    private static FontFamily?      _appFont;

    private static FontFamily AppFont
        => _appFont ??= Application.Current.Resources.TryGetValue("AppFontFamily", out var r) && r is FontFamily f
            ? f : FontFamily.XamlAutoFontFamily;

    public static void RegisterService(HyperLinkService svc) => _hyperLinkSvc = svc;

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
        if (e.NewValue is not string text || string.IsNullOrEmpty(text)) return;
        RenderInto(rtb, text, rtb);
    }

    private static void ParseChildren(InlineCollection inlines, List<MiHoYoToken> tokens, string src,
        ref int pos, MiHoYoTokenKind closeKind, FrameworkElement? anchor)
    {
        while (pos < tokens.Count)
        {
            var tok = tokens[pos];
            if (tok.Kind == MiHoYoTokenKind.Eof || tok.Kind == closeKind) return;

            switch (tok.Kind)
            {
                case MiHoYoTokenKind.Text:
                    pos++;
                    AppendText(inlines, src.AsSpan(tok.Start, tok.End - tok.Start));
                    break;

                case MiHoYoTokenKind.ColorOpen:
                {
                    pos++;
                    var colorStr = src.AsSpan(tok.Start + 8, tok.End - 1 - (tok.Start + 8)).ToString();
                    var span = new Span { Foreground = new SolidColorBrush(ParseHex(colorStr)) };
                    ParseChildren(span.Inlines, tokens, src, ref pos, MiHoYoTokenKind.ColorClose, anchor);
                    if (pos < tokens.Count && tokens[pos].Kind == MiHoYoTokenKind.ColorClose) pos++;
                    inlines.Add(span);
                    break;
                }

                case MiHoYoTokenKind.ItalicOpen:
                {
                    pos++;
                    var span = new Span { FontStyle = FontStyle.Italic };
                    ParseChildren(span.Inlines, tokens, src, ref pos, MiHoYoTokenKind.ItalicClose, anchor);
                    if (pos < tokens.Count && tokens[pos].Kind == MiHoYoTokenKind.ItalicClose) pos++;
                    inlines.Add(span);
                    break;
                }

                case MiHoYoTokenKind.LinkOpen:
                {
                    pos++;
                    var idSpan = src.AsSpan(tok.Start + 6, tok.End - 1 - (tok.Start + 6));
                    var link = new Hyperlink();
                    if (anchor is not null && idSpan.Length > 1 && idSpan[0] == 'N' &&
                        uint.TryParse(idSpan[1..], out uint nId))
                    {
                        link.Click += (_, _) => ShowHyperLinkFlyout(anchor, nId);
                    }
                    ParseChildren(link.Inlines, tokens, src, ref pos, MiHoYoTokenKind.LinkClose, anchor);
                    if (pos < tokens.Count && tokens[pos].Kind == MiHoYoTokenKind.LinkClose) pos++;
                    inlines.Add(link);
                    break;
                }

                case MiHoYoTokenKind.SpritePreset:
                    pos++;
                    break;

                default:
                    pos++;
                    break;
            }
        }
    }

    private static void AppendText(InlineCollection inlines, ReadOnlySpan<char> span)
    {
        while (span.Length > 0)
        {
            int nlIdx      = span.IndexOf('\n');
            int literalIdx = span.IndexOf("\\n");

            int breakIdx, skip;
            if (literalIdx >= 0 && (nlIdx < 0 || literalIdx < nlIdx))
            { breakIdx = literalIdx; skip = 2; }
            else if (nlIdx >= 0)
            { breakIdx = nlIdx; skip = 1; }
            else break;

            if (breakIdx > 0)
                inlines.Add(new Run { Text = span[..breakIdx].ToString() });
            inlines.Add(new LineBreak());
            span = span[(breakIdx + skip)..];
        }
        if (span.Length > 0)
            inlines.Add(new Run { Text = span.ToString() });
    }

    private static void ShowHyperLinkFlyout(FrameworkElement anchor, uint id)
    {
        if (_hyperLinkSvc is null || !_hyperLinkSvc.TryGet(id, out var name, out var desc)) return;

        var panel = new StackPanel { MaxWidth = 300, Spacing = 6 };

        var nameRtb = new RichTextBlock { TextWrapping = TextWrapping.Wrap, FontFamily = AppFont };
        RenderInto(nameRtb, name, null);
        panel.Children.Add(nameRtb);

        if (!string.IsNullOrEmpty(desc))
        {
            var descRtb = new RichTextBlock { TextWrapping = TextWrapping.Wrap, Opacity = 0.85, FontFamily = AppFont };
            RenderInto(descRtb, desc, null);
            panel.Children.Add(descRtb);
        }

        new Flyout { Content = panel, ShouldConstrainToRootBounds = false }
            .ShowAt(anchor, new FlyoutShowOptions { Placement = FlyoutPlacementMode.Auto });
    }

    private static void RenderInto(RichTextBlock rtb, string text, FrameworkElement? anchor)
    {
        rtb.Blocks.Clear();
        if (string.IsNullOrEmpty(text)) return;
        var para = new Paragraph();
        var tokens = MiHoYoLexer.Tokenize(text);
        int pos = 0;
        ParseChildren(para.Inlines, tokens, text, ref pos, MiHoYoTokenKind.Eof, anchor);
        rtb.Blocks.Add(para);
    }

    private static Color ParseHex(string hex8)
        => Color.FromArgb(
            Convert.ToByte(hex8[6..], 16),
            Convert.ToByte(hex8[..2], 16),
            Convert.ToByte(hex8[2..4], 16),
            Convert.ToByte(hex8[4..6], 16));
}
