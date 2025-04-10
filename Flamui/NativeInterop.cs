﻿using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Flamui;

public static unsafe class WindowNative
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
}

public static unsafe class LinuxNative
{
    [DllImport("glfw3.so", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("linux")]
    public static extern void glfwSetWindowContentScaleCallback(IntPtr window, WindowNative.CallbackDelegate callback);

    [DllImport("glfw3.so", CallingConvention = CallingConvention.Cdecl)]
    [SupportedOSPlatform("linux")]
    public static extern void glfwGetWindowContentScale(IntPtr window, float* xScale, float* yScale);
}