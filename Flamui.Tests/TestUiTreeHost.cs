using Silk.NET.GLFW;

namespace Flamui.Tests;

public sealed class TestUiTreeHost : IUiTreeHost
{
    public string ClipboardText;
    public CursorShape CursorShape;

    public TestUiTreeHost(string clipboardText)
    {
        ClipboardText = clipboardText;
    }

    public string GetClipboardText()
    {
        return ClipboardText;
    }

    public void SetClipboardText(string text)
    {
        ClipboardText = text;
    }

    public void SetCursorStyle(CursorShape cursorShape)
    {
        CursorShape = cursorShape;
    }

    public void CloseWindow()
    {
        throw new NotImplementedException();
    }

    public (int width, int height) GetSize()
    {
        return (100, 100);
    }
}