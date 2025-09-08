using System.Reflection;
using System.Runtime.CompilerServices;
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

public struct ScaledFont : IEquatable<ScaledFont>
{
    public Font Font;
    public float PixelSize;
    public float Scale;

    public float Ascent => Font.UnscaledAscent * Scale;
    public float Descent => Font.UnscaledDescent * Scale;
    public float LineGap => Font.UnscaledLineGap * Scale;

    public ScaledFont(Font font, float pixelSize)
    {
        if (font == null)
            throw new Exception();

        Font = font;
        PixelSize = pixelSize;
        Scale = Font.GetScale(pixelSize);
    }

    public float GetAdvanceWith(char c)
    {
        return Font.GetGlyphInfo(c).UnscaledAdvanceWidth * Scale;
    }

    public float GetHeight() => Ascent - Descent;

    public override int GetHashCode()
    {
        return HashCode.Combine(Font?.GetHashCode() ?? 0, PixelSize.GetHashCode());
    }

    public bool Equals(ScaledFont other)
    {
        return Font == other.Font && PixelSize.Equals(other.PixelSize);
    }

    public override bool Equals(object? obj)
    {
        return obj is ScaledFont other && Equals(other);
    }
}

public sealed class Font
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
}

public record struct GlyphCacheHash
{
    public required char Character;
    public required float ResolutionMultiplier;
    public required ScaledFont ScaledFont;
}

public sealed class FontAtlas
{
    // public required ScaledFont Font;
    public required uint AtlasWidth;
    public required uint AtlasHeight;

    public GpuTexture GpuTexture;

    public required LRUCache<GlyphCacheHash, AtlasGlyphInfo> Table;

    private nint GlyphTempMemory;

    public FontAtlas()
    {
        GlyphTempMemory = Marshal.AllocHGlobal(100 * 100);
    }

    public unsafe AtlasGlyphInfo FindGlyphEntry(ScaledFont font, char c, float resolutionMultiplier)
    {
        Unsafe.InitBlock((void*)GlyphTempMemory, 0, 100 * 100);

        var hash = new GlyphCacheHash { Character = c, ResolutionMultiplier = resolutionMultiplier, ScaledFont = font };

        if (Table.TryGet(hash, out var entry))
        {
            return entry;
        }

        entry = Table.GetLeastUsed();

        int ix0 = 0;
        int iy0 = 0;
        int ix1 = 0;
        int iy1 = 0;

        var scale = font.Scale * resolutionMultiplier;
        StbTrueType.stbtt_GetCodepointBitmapBox(font.Font.FontInfo, c, scale, scale, &ix0, &iy0, &ix1, &iy1);
        StbTrueType.stbtt_MakeCodepointBitmap(font.Font.FontInfo, (byte*)GlyphTempMemory, 100, 100, 100, scale, scale, c);

        int width = ix1 - ix0;
        int height = iy1 - iy0;
        int xOff = ix0;
        int yOff = iy0;

        // if (c == 't')
        // {
        //     Console.WriteLine($"Dumping {c}");
        //     var span = new Span<byte>((void*)GlyphTempMemory, 100 * 100);
        //     for (int i = 0; i < 100; i++)
        //     {
        //         for (int j = 0; j < 100; j++)
        //         {
        //             Console.Write(span[i * 100 + j] == 0 ? '0' : '1');
        //         }
        //
        //         Console.WriteLine();
        //     }
        // }


        // Span<byte> tempBuffer = stackalloc byte[100];
        // var s = new Span<byte>((void*)GlyphTempMemory, 100 * 100);
        //
        // for (int i = 0; i < 50; i++)
        // {
        //     var slice1 = s.Slice(i * 100, 100);
        //     var slice2 = s.Slice((100 - i - 1) * 100, 100);
        //     slice1.CopyTo(tempBuffer);
        //     slice2.CopyTo(slice1);
        //     tempBuffer.CopyTo(slice2);
        // }

        //patch texture
        GpuTexture.Gl.BindTexture(TextureTarget.Texture2D, GpuTexture.TextureId);
        GpuTexture.Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GpuTexture.Gl.TexSubImage2D(TextureTarget.Texture2D, 0, entry.AtlasX, entry.AtlasY, 100, 100, PixelFormat.Red, PixelType.UnsignedByte, (void*)GlyphTempMemory);

        Renderer.CheckError2(GpuTexture.Gl);

        var info = new AtlasGlyphInfo
        {
            Height = height / resolutionMultiplier,
            Width = width / resolutionMultiplier,
            XOff = xOff / resolutionMultiplier,
            YOff = yOff / resolutionMultiplier,
            AtlasX = entry.AtlasX,
            AtlasY = entry.AtlasY,
            SlotNumber = entry.SlotNumber,
            AtlasWidth = width,
            AtlasHeight = height,
            // GlyphBoundingBox = bb,
            FontGlyphInfo = font.Font.GetGlyphInfo(c),
            Scale = font.Scale
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

    public float AdvanceWidth => FontGlyphInfo.UnscaledAdvanceWidth * Scale;
    public float LeftSideBearing => FontGlyphInfo.UnscaledLeftSideBearing * Scale;

    // public required GlyphBoundingBox GlyphBoundingBox;
}

public sealed class FontLoader
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

        using var stream = typeof(FontLoader).Assembly.GetManifestResourceStream($"Flamui.Drawing.{fileName}");
        using MemoryStream ms = new MemoryStream();
        stream!.CopyTo(ms);
        return ms.ToArray();
    }

    public static FontAtlas CreateFontAtlas()
    {
        var table = new LRUCache<GlyphCacheHash, AtlasGlyphInfo>(10*10);

        int slotNumber = 0;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                table.Add(new GlyphCacheHash
                {
                    Character = (char)(i * 10 + j),
                    ResolutionMultiplier = i * j,
                    ScaledFont = default
                }, default(AtlasGlyphInfo) with
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