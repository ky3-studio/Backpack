using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

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
        var tokens = Tokenize(text);
        int pos = 0;
        ParseChildren(para.Inlines, tokens, text, ref pos, TokenKind.Eof);
        rtb.Blocks.Add(para);
    }

    private enum TokenKind
    {
        Text, ColorOpen, ColorClose, ItalicOpen, ItalicClose,
        LinkOpen, LinkClose, SpritePreset, Eof,
    }

    private readonly record struct Token(TokenKind Kind, int Start, int End);

    private static List<Token> Tokenize(string s)
    {
        var list = new List<Token>();
        int i = 0;
        while (i < s.Length)
        {
            int start = i;
            if (s[i] == '<')
            {
                if (TryMatch(s, i, "<color=", out int after) && s.IndexOf('>', after) is > 0 and int closeAngle)
                {
                    list.Add(new(TokenKind.ColorOpen, start, closeAngle + 1));
                    i = closeAngle + 1;
                    continue;
                }
                if (TryMatch(s, i, "</color>", out int e1)) { list.Add(new(TokenKind.ColorClose, start, e1)); i = e1; continue; }
                if (TryMatch(s, i, "<i>",      out int e2)) { list.Add(new(TokenKind.ItalicOpen,  start, e2)); i = e2; continue; }
                if (TryMatch(s, i, "</i>",     out int e3)) { list.Add(new(TokenKind.ItalicClose, start, e3)); i = e3; continue; }
            }
            else if (s[i] == '{')
            {
                if (TryMatch(s, i, "{LINK#", out _) && s.IndexOf('}', i + 6) is > 0 and int lEnd)
                {
                    list.Add(new(TokenKind.LinkOpen, start, lEnd + 1));
                    i = lEnd + 1;
                    continue;
                }
                if (TryMatch(s, i, "{/LINK}", out int e4)) { list.Add(new(TokenKind.LinkClose, start, e4)); i = e4; continue; }
                if (TryMatch(s, i, "{SPRITE_PRESET#", out _) && s.IndexOf('}', i + 15) is > 0 and int spEnd)
                {
                    list.Add(new(TokenKind.SpritePreset, start, spEnd + 1));
                    i = spEnd + 1;
                    continue;
                }
            }

            int textEnd = i + 1;
            while (textEnd < s.Length && s[textEnd] is not '<' and not '{')
                textEnd++;
            list.Add(new(TokenKind.Text, start, textEnd));
            i = textEnd;
        }
        list.Add(new(TokenKind.Eof, s.Length, s.Length));
        return list;
    }

    private static bool TryMatch(string s, int pos, string keyword, out int after)
    {
        if (pos + keyword.Length <= s.Length &&
            s.AsSpan(pos, keyword.Length).Equals(keyword.AsSpan(), StringComparison.Ordinal))
        {
            after = pos + keyword.Length;
            return true;
        }
        after = pos;
        return false;
    }

    private static void ParseChildren(InlineCollection inlines, List<Token> tokens, string src,
        ref int pos, TokenKind closeKind)
    {
        while (pos < tokens.Count)
        {
            var tok = tokens[pos];
            if (tok.Kind == TokenKind.Eof || tok.Kind == closeKind) return;

            switch (tok.Kind)
            {
                case TokenKind.Text:
                    pos++;
                    AppendText(inlines, src.AsSpan(tok.Start, tok.End - tok.Start));
                    break;

                case TokenKind.ColorOpen:
                {
                    pos++;
                    var colorStr = src.AsSpan(tok.Start + 8, tok.End - 1 - (tok.Start + 8)).ToString();
                    var span = new Span { Foreground = new SolidColorBrush(ParseHex(colorStr)) };
                    ParseChildren(span.Inlines, tokens, src, ref pos, TokenKind.ColorClose);
                    if (pos < tokens.Count && tokens[pos].Kind == TokenKind.ColorClose) pos++;
                    inlines.Add(span);
                    break;
                }

                case TokenKind.ItalicOpen:
                {
                    pos++;
                    var span = new Span { FontStyle = FontStyle.Italic };
                    ParseChildren(span.Inlines, tokens, src, ref pos, TokenKind.ItalicClose);
                    if (pos < tokens.Count && tokens[pos].Kind == TokenKind.ItalicClose) pos++;
                    inlines.Add(span);
                    break;
                }

                case TokenKind.LinkOpen:
                {
                    pos++;
                    var link = new Hyperlink();
                    ParseChildren(link.Inlines, tokens, src, ref pos, TokenKind.LinkClose);
                    if (pos < tokens.Count && tokens[pos].Kind == TokenKind.LinkClose) pos++;
                    inlines.Add(link);
                    break;
                }

                case TokenKind.SpritePreset:
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

    private static Color ParseHex(string hex8)
        => Color.FromArgb(
            Convert.ToByte(hex8[6..], 16),
            Convert.ToByte(hex8[..2], 16),
            Convert.ToByte(hex8[2..4], 16),
            Convert.ToByte(hex8[4..6], 16));
}
