using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public enum InputType
{
    Text,
    Numeric
}

public static partial class UiExtensions
{
    public static UiText Input(this Ui ui, ref string text, bool hasFocus = false, InputType inputType = InputType.Text, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        //todo, if multiple inputs happen in the same frame, stuff breaks, this hole thing should be handled differently

        var t = ui.Text(text, key, path, line);
        t.ShowCursor = hasFocus;

        if (hasFocus)
        {
            text = TextBoxInputHandler.ProcessInput(text, t.TextLayoutInfo, ui.Window.Input, t.UiTextInfo.Multiline, ref t.CursorPosition, ref t.SelectionStart);
        }

        t.UiTextInfo.Content = text;

        return t;
    }
}
