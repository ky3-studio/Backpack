namespace Backpack.Viewer.Controls;

internal enum MiHoYoTokenKind
{
    Text, ColorOpen, ColorClose, ItalicOpen, ItalicClose,
    LinkOpen, LinkClose, SpritePreset, Eof,
}

internal readonly record struct MiHoYoToken(MiHoYoTokenKind Kind, int Start, int End);

internal static class MiHoYoLexer
{
    public static List<MiHoYoToken> Tokenize(string s)
    {
        var list = new List<MiHoYoToken>();
        int i = 0;
        while (i < s.Length)
        {
            int start = i;
            if (s[i] == '<')
            {
                if (TryMatch(s, i, "<color=", out int after) && s.IndexOf('>', after) is > 0 and int closeAngle)
                {
                    list.Add(new(MiHoYoTokenKind.ColorOpen, start, closeAngle + 1));
                    i = closeAngle + 1;
                    continue;
                }
                if (TryMatch(s, i, "</color>", out int e1)) { list.Add(new(MiHoYoTokenKind.ColorClose,  start, e1)); i = e1; continue; }
                if (TryMatch(s, i, "<i>",      out int e2)) { list.Add(new(MiHoYoTokenKind.ItalicOpen,  start, e2)); i = e2; continue; }
                if (TryMatch(s, i, "</i>",     out int e3)) { list.Add(new(MiHoYoTokenKind.ItalicClose, start, e3)); i = e3; continue; }
            }
            else if (s[i] == '{')
            {
                if (TryMatch(s, i, "{LINK#", out _) && s.IndexOf('}', i + 6) is > 0 and int lEnd)
                {
                    list.Add(new(MiHoYoTokenKind.LinkOpen, start, lEnd + 1));
                    i = lEnd + 1;
                    continue;
                }
                if (TryMatch(s, i, "{/LINK}", out int e4)) { list.Add(new(MiHoYoTokenKind.LinkClose, start, e4)); i = e4; continue; }
                if (TryMatch(s, i, "{SPRITE_PRESET#", out _) && s.IndexOf('}', i + 15) is > 0 and int spEnd)
                {
                    list.Add(new(MiHoYoTokenKind.SpritePreset, start, spEnd + 1));
                    i = spEnd + 1;
                    continue;
                }
            }

            int textEnd = i + 1;
            while (textEnd < s.Length && s[textEnd] is not '<' and not '{')
                textEnd++;
            list.Add(new(MiHoYoTokenKind.Text, start, textEnd));
            i = textEnd;
        }
        list.Add(new(MiHoYoTokenKind.Eof, s.Length, s.Length));
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
}
