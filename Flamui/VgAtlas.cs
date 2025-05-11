using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.PerfTrace;
using Silk.NET.OpenGL;

namespace Flamui;

public struct AtlasEntry
{
    public required int X;
    public required int Y;
    public required uint Width;
    public required uint Height;
}

public record struct AtlasEntryKey(int SvgHash, uint Width, uint Height);

public class VgAtlas
{
    public LRUCache<AtlasEntryKey, AtlasEntry> Table;
    public GpuTexture GpuTexture;
    private readonly Bitmap tempBitmap;

    public unsafe VgAtlas(Renderer renderer)
    {
        const int byteCount = 1000 * 1000 * 4;

        var content = new byte[byteCount];
        fixed (byte* c = content)
        {
            var bitmap = new Bitmap
            {
                Data = new Slice<byte>(c, content.Length),
                Width = 1000,
                Height = 1000,
                BitmapFormat = BitmapFormat.RGBA
            };
            GpuTexture = renderer.UploadTexture(bitmap);
        }

        Table = new LRUCache<AtlasEntryKey, AtlasEntry>(10*10);

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Table.Add(new AtlasEntryKey((char)(i * 10 + j), 0, 0), default(AtlasEntry) with
                {
                    X = i * 100,
                    Y = j * 100,
                });
            }
        }


        tempBitmap = new Bitmap
        {
            Data = new Slice<byte>((byte*)Marshal.AllocHGlobal(byteCount), byteCount),
            Height = 100,
            Width = 100,
            BitmapFormat = BitmapFormat.RGBA
        };
    }

    public unsafe AtlasEntry GetAtlasEntry(int svgHash, Span<byte> vgData, uint width, uint height)
    {
        var hash = new AtlasEntryKey(svgHash, width, height);
        if (Table.TryGet(hash, out var entry))
        {
            return entry;
        }

        using var _ = ConsoleTimer.Time("VG generation");

        entry = Table.GetLeastUsed();

        fixed (byte* tvg = vgData)
        {
            TinyvgBitmap tinyvgBitmap = default;
            var err = TinyVG.tinyvg_render_bitmap(tvg, vgData.Length, TinyvgAntiAlias.X16, width, height, ref tinyvgBitmap);
            if (err != TinyvgError.Success)
            {
                throw new Exception($"Error rendering svg: {err}");
            }

            entry.Width = tinyvgBitmap.Width;
            entry.Height = tinyvgBitmap.Height;

            var bitmap = new Bitmap
            {
                Width = tinyvgBitmap.Width,
                Height = tinyvgBitmap.Height,
                Data = new Slice<byte>((byte*)tinyvgBitmap.Pixels, (int)(tinyvgBitmap.Width * tinyvgBitmap.Height * 4)),
                BitmapFormat = BitmapFormat.RGBA
            };

            //premultiply alpha
            for (int i = 0; i < bitmap.Height * bitmap.Width; i++)
            {
                var a = (float)bitmap.Data[i * 4 + 3] / 255;

                bitmap.Data[i * 4 + 0] = (byte)((float)bitmap.Data[i * 4 + 0] * a);
                bitmap.Data[i * 4 + 1] = (byte)((float)bitmap.Data[i * 4 + 1] * a);
                bitmap.Data[i * 4 + 2] = (byte)((float)bitmap.Data[i * 4 + 2] * a);
            }

            bitmap.CopyTo(tempBitmap);
            // tempBitmap.PrintToConsole();


            GpuTexture.Gl.BindTexture(TextureTarget.Texture2D, GpuTexture.TextureId);
            GpuTexture.Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GpuTexture.Gl.TexSubImage2D(TextureTarget.Texture2D, 0, entry.X, entry.Y, (uint)tempBitmap.Width, (uint)tempBitmap.Height, PixelFormat.Rgba, PixelType.UnsignedByte, (void*)tempBitmap.Data.Items);
        }

        Table.Add(hash, entry);
        return entry;
    }
}