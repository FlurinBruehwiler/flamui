using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Flamui;

public enum BitmapFormat
{
    R,
    RGBA
}

public struct Bitmap
{
    public required uint Width;
    public required uint Height;
    public required Slice<byte> Data;
    public required BitmapFormat BitmapFormat;

    public override int GetHashCode()
    {
        return HashCode.Combine(Width.GetHashCode(), Height.GetHashCode(), Data.GetHashCode());
    }
}

public class TGALoader
{
    public static unsafe Bitmap TgaToBitmap(Span<byte> tgaFile)
    {
        fixed (byte* ptr = tgaFile)
        {
            Debug.Assert(sizeof(ColorMapSpecification) == 5, sizeof(ColorMapSpecification).ToString());
            Debug.Assert(sizeof(ImageSpecification) == 10, sizeof(ImageSpecification).ToString());
            Debug.Assert(sizeof(TGAHeader) == 18, sizeof(TGAHeader).ToString());

            if (tgaFile.Length < sizeof(TGAHeader))
                throw new Exception("Invalid TGA header");

            var header = Marshal.PtrToStructure<TGAHeader>((nint)ptr);

            if (header.ColorMapType != 0)
                throw new Exception("We don't support color maps");

            if (header.ImageType != 2)
                throw new Exception("We only support uncompressed true-color images");

            if (header.ImageSpecification.PixelDepth != 32)
                throw new Exception("We only support 32 bit pixel depth");

            Console.WriteLine(JsonSerializer.Serialize(header, options: new JsonSerializerOptions{ IncludeFields = true, WriteIndented = true}));

            tgaFile = tgaFile.Slice(sizeof(TGAHeader));

            var id = tgaFile.Slice(0, header.IDLength);
            Console.WriteLine($"Header: {Encoding.UTF8.GetString(id)}");

            tgaFile = tgaFile.Slice(header.IDLength);

            var expectedSize = (header.ImageSpecification.ImageWidth * header.ImageSpecification.ImageHeight *
                                header.ImageSpecification.PixelDepth) / 8;

            if (tgaFile.Length != expectedSize)
                throw new Exception("Unexpected Length of TGA file");

            return new Bitmap
            {
                Width = (uint)header.ImageSpecification.ImageWidth,
                Height = (uint)header.ImageSpecification.ImageHeight,
                Data = new Slice<byte>(),
                BitmapFormat = BitmapFormat.RGBA
                // Data = tgaFile.ToArray()
            };
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 18)]
    public struct TGAHeader
    {
        [FieldOffset(0)]
        public byte IDLength; //not used info
        [FieldOffset(1)]
        public byte ColorMapType; //0: no colorMap, 1: hasColorMap
        [FieldOffset(2)]
        public byte ImageType;
        [FieldOffset(3)]
        public ColorMapSpecification ColorMapSpecification;
        [FieldOffset(8)]
        public ImageSpecification ImageSpecification;
    }

    [StructLayout(LayoutKind.Explicit, Size = 5)]
    public struct ColorMapSpecification
    {
        [FieldOffset(0)]
        public short FirstEntryIndex;
        [FieldOffset(2)]
        public short ColorMapLength;
        [FieldOffset(4)]
        public byte ColorMapEntrySize;
    }

    [StructLayout(LayoutKind.Explicit, Size = 10)]
    public struct ImageSpecification
    {
        [FieldOffset(0)]
        public short XOrigin;
        [FieldOffset(2)]
        public short YOrigin;
        [FieldOffset(4)]
        public short ImageWidth;
        [FieldOffset(6)]
        public short ImageHeight;
        [FieldOffset(8)]
        public byte PixelDepth;
        [FieldOffset(9)]
        public byte ImageDescriptor;
    }
}

