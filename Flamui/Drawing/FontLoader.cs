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
    public Slice<byte> Data;
    public required float UnscaledAscent;
    public required float UnscaledDescent;
    public required float UnscaledLineGap;
    public required StbTrueType.stbtt_fontinfo FontInfo;

    public required Dictionary<char, FontGlyphInfo> FontGlyphInfos;

    public float GetScale(float pixelSize)
    {
        return StbTrueType.stbtt_ScaleForPixelHeight(FontInfo, pixelSize);
    }

    public float GetHeight(float scale)
    {
        return UnscaledAscent * scale - UnscaledDescent * scale;
    }

    public float GetCharWidth(char c, float scale)
    {
        if (FontGlyphInfos.TryGetValue(c, out var info))
        {
            return info.UnscaledAdvanceWidth * scale;
        }

        return 0;
    }
}

public class FontAtlas
{
    public required Font Font;
    public required byte[] AtlasBitmap;
    public required int AtlasWidth;
    public required int AtlasHeight;
    public required Dictionary<char, AtlasGlyphInfo> GlyphInfos;

    public required float Scale;
    public GpuTexture GpuTexture;
    public float Ascent => Font.UnscaledAscent * Scale;
    public float Descent => Font.UnscaledDescent * Scale;
    public float LineGap => Font.UnscaledLineGap * Scale;

    public float GetHeight() => Ascent - Descent;

    public float GetCharWidth(char c)
    {
        if (GlyphInfos.TryGetValue(c, out var info))
        {
            return info.FontGlyphInfo.UnscaledAdvanceWidth * Scale;
        }

        return 0;
    }
}

public struct FontGlyphInfo
{
    public required int UnscaledAdvanceWidth;
    public required int UnscaledLeftSideBearing;
}

public struct AtlasGlyphInfo
{
    public required float Scale;
    public required int AtlasX;
    public required int AtlasY;
    public required FontGlyphInfo FontGlyphInfo;

    public required int Width; //Width of the glyph
    public required int Height; //height of the glyph
    public required int XOff;
    public required int YOff;

    public int AdvanceWidth => (int)(FontGlyphInfo.UnscaledAdvanceWidth * Scale);
    public int LeftSideBearing => (int)(FontGlyphInfo.UnscaledLeftSideBearing * Scale);

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
    public static unsafe Font LoadFont(string name)
    {
        var asm = Assembly.GetExecutingAssembly();

        using var stream = asm.GetManifestResourceStream($"Flamui.Drawing.{name}");
        using MemoryStream ms = new MemoryStream();
        stream!.CopyTo(ms);
        var fontData = ms.ToArray();

        var ptr = Marshal.AllocHGlobal(fontData.Length); //todo maybe also free again????
        var slice = new Slice<byte>((byte*)ptr, fontData.Length);
        fontData.CopyTo(slice.Span);

        var info = new StbTrueType.stbtt_fontinfo();

        StbTrueType.stbtt_InitFont(info, slice.Items, 0);

        int ascent = 0;
        int descent = 0;
        int lineGap = 0;
        StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        Dictionary<char, FontGlyphInfo> glyphInfos = new('~' - ' ');

        for (char i = ' '; i < '~'; i++)
        {
            int advanceWidth = 0;
            int leftSideBearing = 0;

            StbTrueType.stbtt_GetCodepointHMetrics(info, i, &advanceWidth, &leftSideBearing);

            glyphInfos[i] = new FontGlyphInfo
            {
                UnscaledAdvanceWidth = advanceWidth,
                UnscaledLeftSideBearing = leftSideBearing
            };
        }

        return new Font
        {
            Name = name,
            Data = slice,
            UnscaledAscent = ascent,
            UnscaledDescent = descent,
            UnscaledLineGap = lineGap,
            FontInfo = info,
            FontGlyphInfos = glyphInfos
        };
    }

    public static unsafe FontAtlas CreateFontAtlas(Font font, float pixelSize)
    {
        var scale = StbTrueType.stbtt_ScaleForPixelHeight(font.FontInfo, pixelSize); //todo multiply by 3 because we do subpixel antialiasing

        int maxCharWidth = 0;
        int maxCharHeight = 0;

        CharInfo[] charInfos = new CharInfo['~' - ' '];

        for (char i = ' '; i < '~'; i++)
        {
            int width = 0;
            int height = 0;
            int xOff = 0;
            int yOff = 0;

            var bitmap = StbTrueType.stbtt_GetCodepointBitmap(font.FontInfo, 0, scale, i, &width, &height, &xOff, &yOff);

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
        atlasSize = RoundUpToNextPowerOfTwo(atlasSize);
        //
        // if (atlasSize % 2 != 0)
        // {
        //     atlasSize++;
        // }
        //todo make atalasSize even number

        byte[] fontAtlasBitmapData = new byte[atlasSize * atlasSize];

        BitRect fontAtlasBitmap = new BitRect
        {
            Data = fontAtlasBitmapData.AsSpan(),
            Height = atlasSize,
            Width = atlasSize
        };
        var glyphInfos = new Dictionary<char, AtlasGlyphInfo>(charInfos.Length);

        int currentCol = 0;
        int currentRow = 0;

        for (var i = 0; i < charInfos.Length; i++)
        {
            var c = charInfos[i];

            var bb = new GlyphBoundingBox();

            StbTrueType.stbtt_GetCodepointBitmapBox(font.FontInfo, i, 0, scale, &bb.x0, &bb.y0, &bb.x1, &bb.y1);

            var bitmap = new BitRect
            {
                Data = c.BitmapAsSpan(),
                Height = c.Height,
                Width = c.Width
            };

            int xOffset = currentCol * maxCharWidth;
            int yOffset = currentRow * maxCharHeight;

            Copy(bitmap, fontAtlasBitmap, xOffset, yOffset);

            glyphInfos[c.Char] = new AtlasGlyphInfo
            {
                Height = c.Height,
                Width = c.Width,
                XOff = c.XOff,
                YOff = c.YOff,
                AtlasX = xOffset,
                AtlasY = yOffset,
                GlyphBoundingBox = bb,
                FontGlyphInfo = font.FontGlyphInfos[c.Char],
                Scale = scale
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

        return new FontAtlas
        {
            Font = font,
            AtlasBitmap = fontAtlasBitmapData,
            AtlasWidth = atlasSize,
            AtlasHeight = atlasSize,
            GlyphInfos = glyphInfos,
            Scale = scale,
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

    public static int RoundUpToNextPowerOfTwo(int number)
    {
        if (number < 1)
            throw new ArgumentException("Number must be greater than zero.");

        number--;
        number |= number >> 1;
        number |= number >> 2;
        number |= number >> 4;
        number |= number >> 8;
        number |= number >> 16; // Works up to 32-bit integers
        return number + 1;
    }
}