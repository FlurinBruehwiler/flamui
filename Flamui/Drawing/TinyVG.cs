using System.Runtime.InteropServices;
using Microsoft.DotNet.PlatformAbstractions;

namespace Flamui.Drawing;

public enum TinyvgError : int
{
    Success = 0,
    OutOfMemory = 1,
    IO = 2,
    InvalidData = 3,
    Unsupported = 4
}

public enum TinyvgAntiAlias : int
{
    None = 1,
    X4 = 2,
    X9 = 3,
    X16 = 4,
    X25 = 6,
    X49 = 7,
    X64 = 8
}

[StructLayout(LayoutKind.Sequential)]
public struct TinyvgOutStream
{
    public IntPtr Context;
    public IntPtr Write;
}

[StructLayout(LayoutKind.Sequential)]
public struct TinyvgBitmap
{
    public uint Width;
    public uint Height;
    public IntPtr Pixels;
}

public static class TinyVG
{
    public static unsafe TinyvgError tinyvg_render_svg(
        byte[] tvgData,
        nint tvgLength,
        ref TinyvgOutStream target
    )
    {
        if (OperatingSystem.IsLinux())
        {
            return LinuxNative.tinyvg_render_svg(tvgData, tvgLength, ref target);
        }
        else if (OperatingSystem.IsWindows())
        {
            return WindowsNative.tinyvg_render_svg(tvgData, tvgLength, ref target);
        }
        else
        {
            throw new NotImplementedException("Open an Issue on the github and we may fix it (sometime next century)");
        }
    }

    public static unsafe TinyvgError tinyvg_render_bitmap(
        byte* tvgData,
        nint tvgLength,
        TinyvgAntiAlias antiAlias,
        uint width,
        uint height,
        ref TinyvgBitmap bitmap
    )
    {
        if (OperatingSystem.IsLinux())
        {
            return LinuxNative.tinyvg_render_bitmap(tvgData, tvgLength, antiAlias, width, height, ref bitmap);
        }
        else if (OperatingSystem.IsWindows())
        {
            return WindowsNative.tinyvg_render_bitmap(tvgData, tvgLength, antiAlias, width, height, ref bitmap);
        }
        else
        {
            throw new NotImplementedException("Open an Issue on the github and we may fix it (sometime next century)");
        }
    }

    public static void tinyvg_free_bitmap(ref TinyvgBitmap bitmap)
    {
        if (OperatingSystem.IsLinux())
        {
            LinuxNative.tinyvg_free_bitmap(ref bitmap);
        }
        else if (OperatingSystem.IsWindows())
        {
            WindowsNative.tinyvg_free_bitmap(ref bitmap);
        }
    }

    //https://tinyvg.tech/download/specification.pdf
    public static (float width, float height) ParseHeader(Span<byte> vgFile)
    {
        if (!BitConverter.IsLittleEndian)
            throw new Exception("Not supported on this system");

        if (vgFile[0] != 0x72 || vgFile[1] != 0x56 || vgFile[2] != 1)
            throw new Exception("Invalid Header");

        var b = vgFile[3];
        var scale = (b & 0b00001111) >> 0;
        var colorEncoding = (b & 0b00110000) >> 4;
        var coordinateRange = (b & 0b11000000) >> 6;

        if (colorEncoding != 0)
            throw new Exception("Only rgba supported");

        if (coordinateRange != 0)
            throw new Exception("Only support 16 bit coordinate ranges");

        var width = (float)BitConverter.ToUInt16(vgFile.Slice(4)) / scale;
        var height = (float)BitConverter.ToUInt16(vgFile.Slice(6)) / scale;

        return (width, height);
    }
}