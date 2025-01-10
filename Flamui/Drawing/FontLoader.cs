using System.Reflection;
using System.Runtime.InteropServices;
using StbTrueTypeSharp;

namespace Flamui.Drawing;

//http://arkanis.de/weblog/2023-08-14-simple-good-quality-subpixel-text-rendering-in-opengl-with-stb-truetype-and-dual-source-blending#gl45-subpixel-text-rendering-demo

public struct GlyphBoundingBox
{
    public int x0;
    public int y0;
    public int x1;
    public int y1;
}

public class Font
{
    public required string Name;
    public required byte[] AtlasBitmap;
    public required int AtlasWidth;
    public required int AtlasHeight;
    public required Dictionary<char, GlyphInfo> GlyphInfos;

    public required float Ascent;
    public required float Descent;
    public required float LineGap;

    public float GetHeight() => Ascent - Descent;

    public float GetCharWidth(char c)
    {
        if (GlyphInfos.TryGetValue(c, out var info))
        {
            return info.AdvanceWidth;
        }

        return 0;
    }
}

public struct GlyphInfo
{
    public required int AtlasX;
    public required int AtlasY;


    public required int Width; //Width of the glyph
    public required int Height; //height of the glyph
    public required int XOff;
    public required int YOff;

    public required int AdvanceWidth;
    public required int LeftSideBearing;

    public required GlyphBoundingBox GlyphBoundingBox;
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
    public static unsafe Font LoadFont(string name, int pixelSize)
    {
        var info = new StbTrueType.stbtt_fontinfo();

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"Flamui.Drawing.{name}");
        using MemoryStream ms = new MemoryStream();
        stream!.CopyTo(ms);
        var fontData = ms.ToArray();

        var ptr = Marshal.AllocHGlobal(fontData.Length);
        var unmanagedSpan = new Span<byte>((byte*)ptr, fontData.Length);
        fontData.CopyTo(unmanagedSpan);

        StbTrueType.stbtt_InitFont(info, (byte*)ptr, 0);

        CharInfo[] charInfos = new CharInfo['~' - ' '];

        var scale = StbTrueType.stbtt_ScaleForPixelHeight(info, pixelSize); //todo multiply by 3 because we do subpixel antialiasing

        int ascent = 0;
        int descent = 0;
        int lineGap = 0;
        StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        int maxCharWidth = 0;
        int maxCharHeight = 0;

        for (char i = ' '; i < '~'; i += '\x1')
        {
            int width = 0;
            int height = 0;
            int xOff = 0;
            int yOff = 0;

            var bitmap = StbTrueType.stbtt_GetCodepointBitmap(info, 0, scale, i, &width, &height, &xOff, &yOff);

            maxCharHeight = Math.Max(maxCharHeight, height);
            maxCharWidth = Math.Max(maxCharWidth, width);

            charInfos[i - ' '] = new CharInfo
            {
                Bitmap = bitmap,
                Width = width,
                Height = height,
                XOff = xOff,
                YOff = yOff,
                Char = i,
            };
        }

        int rowColCount = (int)Math.Ceiling(Math.Sqrt(charInfos.Length));
        int minAtlasWidth = rowColCount * maxCharHeight;
        int minAtlasHeight = rowColCount * maxCharWidth;
        int atlasSize = Math.Max(minAtlasHeight, minAtlasWidth);
        if (atlasSize % 2 != 0)
        {
            atlasSize++;
        }
        //todo make atalasSize even number

        byte[] fontAtlasBitmapData = new byte[atlasSize * atlasSize];

        BitRect fontAtlasBitmap = new BitRect
        {
            Data = fontAtlasBitmapData.AsSpan(),
            Height = atlasSize,
            Width = atlasSize
        };
        var glyphInfos = new Dictionary<char, GlyphInfo>(charInfos.Length);

        int currentCol = 0;
        int currentRow = 0;

        for (var i = 0; i < charInfos.Length; i++)
        {
            var c = charInfos[i];

            var bb = new GlyphBoundingBox();

            StbTrueType.stbtt_GetCodepointBitmapBox(info, i, 0, scale, &bb.x0, &bb.y0, &bb.x1, &bb.y1);

            int advanceWidth = 0;
            int leftSideBearing = 0;

            StbTrueType.stbtt_GetCodepointHMetrics(info, i, &advanceWidth, &leftSideBearing);

            var bitmap = new BitRect
            {
                Data = c.BitmapAsSpan(),
                Height = c.Height,
                Width = c.Width
            };

            int xOffset = currentCol * maxCharWidth;
            int yOffset = currentRow * maxCharHeight;

            Copy(bitmap, fontAtlasBitmap, xOffset, yOffset);

            glyphInfos[c.Char] = new GlyphInfo
            {
                Height = c.Height,
                Width = c.Width,
                XOff = c.XOff,
                YOff = c.YOff,
                AtlasX = xOffset,
                AtlasY = yOffset,
                GlyphBoundingBox = bb,
                AdvanceWidth = (int)(advanceWidth * scale),
                LeftSideBearing = (int)(leftSideBearing * scale)
            };

            if (currentCol == rowColCount - 1)
            {
                currentCol = 0;
                currentRow++;
            }
            else
            {
                currentCol++;
            }
        }

        for (var i = 0; i < fontAtlasBitmapData.Length; i++) //correct upwards for clearer text, not sure why we need to do it...
        {
            ref var b = ref fontAtlasBitmapData[i];

            var f = (double)b;
            f = 255 * Math.Pow(f / 255, 0.5f);
            b = (byte)f;
        }

        return new Font
        {
            Name = name,
            AtlasBitmap = fontAtlasBitmapData,
            AtlasWidth = atlasSize,
            AtlasHeight = atlasSize,
            GlyphInfos = glyphInfos,
            Ascent = ascent * scale,
            Descent = descent * scale,
            LineGap = lineGap * scale
        };
    }

    private static void Copy(BitRect src, BitRect dest, int destX, int destY)
    {
        for (int i = 0; i < src.Height; i++)
        {
            var srcSpan = src.Data.Slice(src.GetOffset(0, i), src.Width);
            var destSpan = dest.Data.Slice(dest.GetOffset(destX, destY + i), src.Width);

            srcSpan.CopyTo(destSpan);
        }
    }

    ref struct BitRect
    {
        public int Width;
        public int Height;
        public Span<byte> Data;

        public int GetOffset(int x, int y)
        {
            return y * Width + x;
        }
    }
}