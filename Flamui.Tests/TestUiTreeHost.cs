namespace Flamui.Test;

public class TestUiTreeHost : IUiTreeHost
{
    private readonly string _clipboardText;

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
        throw new NotImplementedException();
    }
}