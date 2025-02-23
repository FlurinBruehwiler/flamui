using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
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

public struct ScaledFont
{
    public Font Font;
    public float PixelSize;
    public float Scale;

    public float Ascent => Font.UnscaledAscent * Scale;
    public float Descent => Font.UnscaledDescent * Scale;
    public float LineGap => Font.UnscaledLineGap * Scale;

    public ScaledFont(Font font, float pixelSize)
    {
        Font = font;
        PixelSize = pixelSize;
        Scale = Font.GetScale(pixelSize);
    }

    public float GetCharWidth(char c)
    {
        return Font.GetGlyphInfo(c).UnscaledAdvanceWidth * Scale;
    }

    public int GetAdvanceWith(char c)
    {
        return (int)(Font.GetGlyphInfo(c).UnscaledAdvanceWidth * Scale);
    }

    public float GetHeight() => Ascent - Descent;

    public override int GetHashCode()
    {
        return HashCode.Combine(Font.GetHashCode(), PixelSize.GetHashCode());
    }
}

public class Font
{
    public required string Name;
    public Slice<byte> Data;
    public required float UnscaledAscent;
    public required float UnscaledDescent;
    public required float UnscaledLineGap;
    public required StbTrueType.stbtt_fontinfo FontInfo;

    private Dictionary<char, FontGlyphInfo> fontGlyphInfos = [];

    public unsafe FontGlyphInfo GetGlyphInfo(char c)
    {
        if (fontGlyphInfos.TryGetValue(c, out var info))
            return info;

        int advanceWidth = 0;
        int leftSideBearing = 0;

        StbTrueType.stbtt_GetCodepointHMetrics(FontInfo, c, &advanceWidth, &leftSideBearing);

        info = new FontGlyphInfo
        {
            UnscaledAdvanceWidth = advanceWidth,
            UnscaledLeftSideBearing = leftSideBearing
        };

        fontGlyphInfos[c] = info;
        return info;
    }

    public float GetScale(float pixelSize)
    {
        return StbTrueType.stbtt_ScaleForPixelHeight(FontInfo, pixelSize);
    }

    public float GetHeight(float scale)
    {
        return UnscaledAscent * scale - UnscaledDescent * scale;
    }

    // public float GetCharWidth(char c, float scale)
    // {
    //     if (FontGlyphInfos.TryGetValue(c, out var info))
    //     {
    //         return info.UnscaledAdvanceWidth * scale;
    //     }
    //
    //     return 0;
    // }
}

public class FontAtlas
{
    public required ScaledFont Font;
    public required int AtlasWidth;
    public required int AtlasHeight;

    public GpuTexture GpuTexture;

    public required LRUCache<int, AtlasGlyphInfo> Table;

    public unsafe AtlasGlyphInfo FindGlyphEntry(char c, float resolutionMultiplier)
    {
        var hash = HashCode.Combine(c, resolutionMultiplier);

        if (Table.TryGet(hash, out var entry))
        {
            return entry;
        }

        entry = Table.GetLeastUsed();

        int width = 0;
        int height = 0;
        int xOff = 0;
        int yOff = 0;

        var bitmap = StbTrueType.stbtt_GetCodepointBitmap(Font.Font.FontInfo, 0, Font.Scale * resolutionMultiplier, c,
            &width, &height, &xOff, &yOff);
        var bitmapSpan = new Span<byte>(bitmap, width * height);

        for (var i = 0; i < bitmapSpan.Length; i++) //correct upwards for clearer text, not sure why we need to do it...
        {
            ref var b = ref bitmapSpan[i];

            var f = (double)b;
            f = 255 * Math.Pow(f / 255, 0.5f);
            b = (byte)f;
        }

        GpuTexture.Gl.BindTexture(TextureTarget.Texture2D, GpuTexture.TextureId);
        GpuTexture.Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GpuTexture.Gl.TexSubImage2D(TextureTarget.Texture2D, 0, entry.AtlasX, entry.AtlasY, (uint)width, (uint)height, PixelFormat.Red, PixelType.UnsignedByte, (void*)bitmap);

        Console.WriteLine($"Uploading {c} at {entry.AtlasX},{entry.AtlasY}");

        var info = new AtlasGlyphInfo
        {
            Height = height / resolutionMultiplier,
            Width = width / resolutionMultiplier,
            XOff = xOff / resolutionMultiplier,
            YOff = yOff / resolutionMultiplier,
            AtlasX = entry.AtlasX,
            AtlasY = entry.AtlasY,
            AtlasWidth = width,
            AtlasHeight = height,
            // GlyphBoundingBox = bb,
            FontGlyphInfo = Font.Font.GetGlyphInfo(c),
            Scale = Font.Scale
        };

        Table.Add(hash, info);

        return info;
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
    public required int AtlasWidth;
    public required int AtlasHeight;
    public required FontGlyphInfo FontGlyphInfo;

    public required float Width; //Width of the glyph
    public required float Height; //height of the glyph
    public required float XOff;
    public required float YOff;

    public int AdvanceWidth => (int)(FontGlyphInfo.UnscaledAdvanceWidth * Scale);
    public int LeftSideBearing => (int)(FontGlyphInfo.UnscaledLeftSideBearing * Scale);

    // public required GlyphBoundingBox GlyphBoundingBox;
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

        // Console.WriteLine($"Allocating {((float)fontData.Length)/1000/1000} MB");
        var ptr = Marshal.AllocHGlobal(fontData.Length); //todo maybe also free again????
        var slice = new Slice<byte>((byte*)ptr, fontData.Length);
        fontData.CopyTo(slice.Span);

        var info = new StbTrueType.stbtt_fontinfo();

        StbTrueType.stbtt_InitFont(info, slice.Items, 0);

        int ascent = 0;
        int descent = 0;
        int lineGap = 0;
        StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        return new Font
        {
            Name = name,
            Data = slice,
            UnscaledAscent = ascent,
            UnscaledDescent = descent,
            UnscaledLineGap = lineGap,
            FontInfo = info,
        };
    }

    public static FontAtlas CreateFontAtlas(ScaledFont scaledFont)
    {
        var table = new LRUCache<int, AtlasGlyphInfo>(10*10);

        for (int i = 1; i < 11; i++)
        {
            for (int j = 1; j < 11; j++)
            {
                table.Add((int.MaxValue - i - 100 * j).GetHashCode(), default(AtlasGlyphInfo) with
                {
                    AtlasX = i * 100,
                    AtlasY = j * 100,
                });
            }
        }

        return new FontAtlas
        {
            Font = scaledFont,
            // AtlasBitmap = fontAtlasBitmapData,
            AtlasWidth = 1024,
            AtlasHeight = 1024,
            Table = table
            // GlyphInfos = glyphInfos,
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