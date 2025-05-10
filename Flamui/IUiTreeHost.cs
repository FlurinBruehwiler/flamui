namespace Flamui;

//This is either a native window, a mock for testing, or the browser
public interface IUiTreeHost
{
    string GetClipboardText();
    void SetClipboardText(string text);
}