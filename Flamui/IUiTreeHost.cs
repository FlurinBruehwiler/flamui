using Silk.NET.GLFW;
using Silk.NET.Windowing;

namespace Flamui;

//This is either a native window, a mock for testing, or the browser
public interface IUiTreeHost
{
    string GetClipboardText();
    void SetClipboardText(string text);
}

public class NativeUiTreeHost : IUiTreeHost
{
    private readonly Glfw _glfw;
    private readonly IWindow _window;

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
}