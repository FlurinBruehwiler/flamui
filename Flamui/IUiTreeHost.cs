
namespace Flamui;

//This is either a native window, a mock for testing, or the browser
public interface IUiTreeHost
{
    string GetClipboardText();
    void SetClipboardText(string text);

    //sets the cursor style across once, remains across frames
    void SetCursorStyle(CursorShape cursorShape);

    void CloseWindow();

    (int width, int height) GetSize();
}

