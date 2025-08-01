using Silk.NET.GLFW;

namespace Flamui.Tests;

public sealed class TestUiTreeHost : IUiTreeHost
{
    private string _clipboardText;
    private CursorShape _cursorShape;

    public TestUiTreeHost(string clipboardText)
    {
        _clipboardText = clipboardText;
    }

    public string GetClipboardText()
    {
        return _clipboardText;
    }

    public void SetClipboardText(string text)
    {
        _clipboardText = text;
    }

    public void SetCursorStyle(CursorShape cursorShape)
    {
        _cursorShape = cursorShape;
    }
}