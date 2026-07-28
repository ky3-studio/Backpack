using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Backpack.Viewer.Services;

internal static partial class GfxLoader
{
    private static string HashedName(string url)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToHexStringLower(hash) + ".png";
    }

    private static bool IsValid(string path)
        => File.Exists(path) && new FileInfo(path).Length > 0 &&
           DetectFormat(path) != ImageFormat.Unknown;

    private static bool HasSrgbChunk(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            Span<byte> header = stackalloc byte[64];
            int read = fs.Read(header);
            if (DetectFormat(header[..Math.Min(12, read)]) != ImageFormat.Png) return true;
            for (int i = 8; i < read - 4; i++)
            {
                if (header.Slice(i, 4).SequenceEqual("sRGB"u8)) return true;
                if (header.Slice(i, 4).SequenceEqual("IDAT"u8)) return false;
            }
            return false;
        }
        catch { return true; }
    }

    private static uint PngCrc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }
        return ~crc;
    }

    private static void InjectSrgbChunk(string path)
    {
        const int insertAt = 33;

        var data = File.ReadAllBytes(path);
        if (DetectFormat(data.AsSpan(0, Math.Min(12, data.Length))) != ImageFormat.Png) return;
        if (data.Length < insertAt) return;

        ReadOnlySpan<byte> typeAndData = "sRGB\0"u8;
        var crcVal = PngCrc32(typeAndData);

        byte[] chunk =
        [
            0x00, 0x00, 0x00, 0x01,
            (byte)'s', (byte)'R', (byte)'G', (byte)'B',
            0x00,
            (byte)(crcVal >> 24), (byte)(crcVal >> 16),
            (byte)(crcVal >> 8),  (byte)crcVal,
        ];

        var result = new byte[data.Length + chunk.Length];
        Buffer.BlockCopy(data,  0,        result, 0,                       insertAt);
        Buffer.BlockCopy(chunk, 0,        result, insertAt,                chunk.Length);
        Buffer.BlockCopy(data,  insertAt, result, insertAt + chunk.Length, data.Length - insertAt);

        File.WriteAllBytes(path, result);
    }
}
