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

public record struct GlyphCacheHash
{
    public char Character;
    public float ResolutionMultiplier;
}

public class FontAtlas
{
    public required ScaledFont Font;
    public required uint AtlasWidth;
    public required uint AtlasHeight;

    public GpuTexture GpuTexture;

    public required LRUCache<GlyphCacheHash, AtlasGlyphInfo> Table;

    private nint GlyphTempMemory;

    public FontAtlas()
    {
        GlyphTempMemory = Marshal.AllocHGlobal(100 * 100);
    }

    public unsafe AtlasGlyphInfo FindGlyphEntry(char c, float resolutionMultiplier)
    {
        var hash = new GlyphCacheHash { Character = c, ResolutionMultiplier = resolutionMultiplier };

        if (Table.TryGet(hash, out var entry))
        {
            return entry;
        }

        entry = Table.GetLeastUsed();

        int ix0 = 0;
        int iy0 = 0;
        int ix1 = 0;
        int iy1 = 0;

        var scale = Font.Scale * resolutionMultiplier;
        StbTrueType.stbtt_GetCodepointBitmapBox(Font.Font.FontInfo, c, scale, scale, &ix0, &iy0, &ix1, &iy1);
        StbTrueType.stbtt_MakeCodepointBitmap(Font.Font.FontInfo, (byte*)GlyphTempMemory, 100, 100, 100, scale, scale, c);

        int width = ix1 - ix0;
        int height = iy1 - iy0;
        int xOff = ix0;
        int yOff = iy0;

        // var bitmapSpan = new Span<byte>((void*)GlyphTempMemory, 100 * 100);

        // Console.WriteLine($"glyph cache miss: {c}:{resolutionMultiplier}, inserting into slot {entry.SlotNumber}");

        // for (var i = 0; i < bitmapSpan.Length; i++) //correct upwards for clearer text, not sure why we need to do it...
        // {
        //     ref var b = ref bitmapSpan[i];
        //
        //     var f = (double)b;
        //     f = 255 * Math.Pow(f / 255, 0.5f);
        //     b = (byte)f;
        // }

        GpuTexture.Gl.BindTexture(TextureTarget.Texture2D, GpuTexture.TextureId);
        GpuTexture.Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GpuTexture.Gl.TexSubImage2D(TextureTarget.Texture2D, 0, entry.AtlasX, entry.AtlasY, (uint)100, (uint)100, PixelFormat.Red, PixelType.UnsignedByte, (void*)GlyphTempMemory);

        var info = new AtlasGlyphInfo
        {
            Height = (float)height / resolutionMultiplier,
            Width = (float)width / resolutionMultiplier,
            XOff = (float)xOff / resolutionMultiplier,
            YOff = (float)yOff / resolutionMultiplier,
            AtlasX = entry.AtlasX,
            AtlasY = entry.AtlasY,
            SlotNumber = entry.SlotNumber,
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
    public required int SlotNumber;

    public int AdvanceWidth => (int)(FontGlyphInfo.UnscaledAdvanceWidth * Scale);
    public int LeftSideBearing => (int)(FontGlyphInfo.UnscaledLeftSideBearing * Scale);

    // public required GlyphBoundingBox GlyphBoundingBox;
}

public class FontLoader
{
    public static unsafe Font LoadFont(string name)
    {
        var fontData = GetFont(name);

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

    private static byte[] GetFont(string fileName)
    {
        var windowsFontLocation = Path.Combine(@"C:\Windows\Fonts\", fileName);
        if (File.Exists(windowsFontLocation))
        {
            return File.ReadAllBytes(windowsFontLocation);
        }

        var asm = Assembly.GetExecutingAssembly();

        using var stream = asm.GetManifestResourceStream($"Flamui.Drawing.{fileName}");
        using MemoryStream ms = new MemoryStream();
        stream!.CopyTo(ms);
        return ms.ToArray();
    }

    public static FontAtlas CreateFontAtlas(ScaledFont scaledFont)
    {
        var table = new LRUCache<GlyphCacheHash, AtlasGlyphInfo>(10*10);

        int slotNumber = 0;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                table.Add(new GlyphCacheHash{ Character = (char)(i * 10 + j), ResolutionMultiplier = i * j}, default(AtlasGlyphInfo) with
                {
                    AtlasX = i * 100,
                    AtlasY = j * 100,
                    SlotNumber = slotNumber
                });

                slotNumber++;
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