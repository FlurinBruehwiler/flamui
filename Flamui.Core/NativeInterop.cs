using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Flamui.Drawing;

namespace Flamui;

public static unsafe class WindowsNative
{
    [DllImport("glfw3.dll", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("windows")]
    public static extern void glfwSetWindowContentScaleCallback(IntPtr window, CallbackDelegate callback);

    [DllImport("glfw3.dll", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("windows")]
    public static extern void glfwGetWindowContentScale(IntPtr window, float* xScale, float* yScale);

    [DllImport("dwmapi.dll", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("windows")]
    public static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CallbackDelegate(IntPtr window, float xScale, float yScale);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetProcessWorkingSetSize(
        IntPtr hProcess,
        UIntPtr dwMinimumWorkingSetSize,
        UIntPtr dwMaximumWorkingSetSize);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(
        IntPtr hWndChild,
        IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

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

public static unsafe class LinuxNative
{
    [DllImport("libglfw.so.3", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("linux")]
    public static extern void glfwSetWindowContentScaleCallback(IntPtr window, WindowsNative.CallbackDelegate callback);

    [DllImport("libglfw.so.3", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("linux")]
    public static extern void glfwGetWindowContentScale(IntPtr window, float* xScale, float* yScale);

    [DllImport("libtinyvg.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe TinyvgError tinyvg_render_svg(
        byte[] tvgData,
        nint tvgLength,
        ref TinyvgOutStream target
    );

    [DllImport("libtinyvg.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe TinyvgError tinyvg_render_bitmap(
        byte* tvgData,
        nint tvgLength,
        TinyvgAntiAlias antiAlias,
        uint width,
        uint height,
        ref TinyvgBitmap bitmap
    );

    [DllImport("libtinyvg.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void tinyvg_free_bitmap(ref TinyvgBitmap bitmap);
}