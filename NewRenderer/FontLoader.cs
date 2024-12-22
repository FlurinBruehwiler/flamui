using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using StbTrueTypeSharp;

namespace NewRenderer;

public class Font
{
    public required string Name;
    public required byte[] AtlasBitmap;
    public required int AtlasWidth;
    public required int AtlasHeight;
    public required Dictionary<char, GlyphInfo> GlyphInfos;

}

public struct GlyphInfo
{
    public required int XAtlasOffset; //the TopLeft Coordinate of the Glyph within the FontAtlas
    public required int Width; //Width of the glyph
    public required int Height; //height of the glyph
    public required int XOff;
    public required int YOff;
}

public unsafe struct CharInfo
{
    public required byte* Bitmap;
    public required int Width;
    public required int Height;
    public required int XOff;
    public required int YOff;
    public required char Char;

    public Span<byte> BitmapAsSpan()
    {
        return new Span<byte>(Bitmap, Width * Height);
    }
}

public class FontLoader
{
    public static unsafe Font LoadFont(string name)
    {
        var info = new StbTrueType.stbtt_fontinfo();

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"NewRenderer.{name}");
        using MemoryStream ms = new MemoryStream();
        stream!.CopyTo(ms);
        var fontData = ms.ToArray();

        var ptr = Marshal.AllocHGlobal(fontData.Length);
        var unmanagedSpan = new Span<byte>((byte*)ptr, fontData.Length);
        fontData.CopyTo(unmanagedSpan);

        StbTrueType.stbtt_InitFont(info, (byte*)ptr, 0);

        CharInfo[] charInfos = new CharInfo['~' - '!'];

        for (char i = '!'; i < '~'; i += '\x1')
        {
            int width = 0;
            int height = 0;
            int xOff = 0;
            int yOff = 0;
            var bitmap = StbTrueType.stbtt_GetCodepointBitmap(info, 0, StbTrueType.stbtt_ScaleForPixelHeight(info, 20), i, &width, &height, &xOff, &yOff);
            charInfos[i - '!'] = new CharInfo
            {
                Bitmap = bitmap,
                Width = width,
                Height = height,
                XOff = xOff,
                YOff = yOff,
                Char = i
            };
        }

        var maxHeight = charInfos.Max(x => x.Height);
        var totalWidth = charInfos.Sum(x => x.Width);

        byte[] fontAtlasBitmap = new byte[maxHeight * totalWidth];
        var glyphInfos = new Dictionary<char, GlyphInfo>(charInfos.Length);

        int currentXOffset = 0;

        for (var i = 0; i < charInfos.Length; i++)
        {
            var c = charInfos[i];

            glyphInfos[c.Char] = new GlyphInfo
            {
                Height = c.Height,
                Width = c.Width,
                XOff = c.XOff,
                YOff = c.YOff,
                XAtlasOffset = currentXOffset
            };

            currentXOffset += c.Width;

            var bitmap = c.BitmapAsSpan();

            for (int j = 0; j < c.Height; j++)
            {
                var destSpan = fontAtlasBitmap.AsSpan(j * totalWidth + currentXOffset);
                var srcSpan = bitmap.Slice(j * c.Width, c.Width);

                srcSpan.CopyTo(destSpan);
            }
        }

        return new Font
        {
            Name = name,
            AtlasBitmap = fontAtlasBitmap,
            AtlasWidth = totalWidth,
            AtlasHeight = maxHeight,
            GlyphInfos = glyphInfos
        };
    }
}