using System.Runtime.InteropServices;

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
    [DllImport("tinyvg.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe TinyvgError tinyvg_render_svg(
        byte[] tvgData,
        nint tvgLength,
        ref TinyvgOutStream target
    );

    [DllImport("tinyvg.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe TinyvgError tinyvg_render_bitmap(
        byte* tvgData,
        nint tvgLength,
        TinyvgAntiAlias antiAlias,
        uint width,
        uint height,
        ref TinyvgBitmap bitmap
    );

    [DllImport("tinyvg.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void tinyvg_free_bitmap(ref TinyvgBitmap bitmap);
}