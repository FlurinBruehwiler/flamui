using Silk.NET.GLFW;
using Silk.NET.Windowing;

namespace Flamui;

//This is either a native window, a mock for testing, or the browser
public interface IUiTreeHost
{
    string GetClipboardText();
    void SetClipboardText(string text);
    void SetCursorStyle(CursorShape cursorShape);
}

public sealed class NativeUiTreeHost : IUiTreeHost
{
    private readonly Glfw _glfw;
    private readonly IWindow _window;
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
            cursor = _glfw.CreateStandardCursor(cursorShape);
        }

        currentCursor = cursorShape;
        _glfw.SetCursor((WindowHandle*)_window.Handle, cursor);
    }
}