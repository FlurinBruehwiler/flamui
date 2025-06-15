namespace Flamui.Tests;

public sealed class TestUiTreeHost : IUiTreeHost
{
    private string _clipboardText;

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
}