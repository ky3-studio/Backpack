 using System.IO;

namespace Backpack.Viewer.Services;

internal static partial class GfxLoader
{
    private enum ImageFormat { Unknown, Png, Webp, Jpeg }

    private static ImageFormat DetectFormat(ReadOnlySpan<byte> hdr)
    {
        if (hdr.Length >= 4 && hdr[0] == 0x89 && hdr[1..4].SequenceEqual("PNG"u8))
            return ImageFormat.Png;
        if (hdr.Length >= 12 && hdr[..4].SequenceEqual("RIFF"u8) && hdr[8..12].SequenceEqual("WEBP"u8))
            return ImageFormat.Webp;
        if (hdr.Length >= 3 && hdr[0] == 0xFF && hdr[1] == 0xD8 && hdr[2] == 0xFF)
            return ImageFormat.Jpeg;
        return ImageFormat.Unknown;
    }

    private static ImageFormat DetectFormat(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            Span<byte> hdr = stackalloc byte[12];
            int read = fs.Read(hdr);
            return DetectFormat(hdr[..read]);
        }
        catch { return ImageFormat.Unknown; }
    }
}
