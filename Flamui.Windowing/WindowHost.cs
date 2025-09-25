using Flamui;
using Silk.NET.GLFW;
using Silk.NET.Windowing;

namespace Flamui.Windowing;

public sealed class NativeUiTreeHost : IUiTreeHost
{
    public readonly Glfw _glfw;
    public readonly IWindow _window;
    private Dictionary<CursorShape, IntPtr> CursorCache = [];
    private CursorShape currentCursor = CursorShape.Arrow;

    public NativeUiTreeHost(IWindow window, Glfw glfw)
    {
        _glfw = glfw;
        _window = window;
    }

    public unsafe string GetClipboardText()
    {
        return _glfw.GetClipboardString((WindowHandle*)_window.Handle);
    }

    public unsafe void SetClipboardText(string text)
    {
        _glfw.SetClipboardString((WindowHandle*)_window.Handle, text);
    }

    public unsafe void SetCursorStyle(CursorShape cursorShape)
    {
        if (cursorShape == currentCursor)
            return;

        Cursor* cursor;

        if (CursorCache.TryGetValue(cursorShape, out var cachedCursor))
        {
            cursor = (Cursor*)cachedCursor;
        }
        else
        {
            cursor = _glfw.CreateStandardCursor(cursorShape.ToSilk());
        }

        currentCursor = cursorShape;
        _glfw.SetCursor((WindowHandle*)_window.Handle, cursor);

    }


    public void CloseWindow()
    {
        _window.Close();
    }

    public (int width, int height) GetSize()
    {
        return (_window.Size.X, _window.Size.Y);
    }
}