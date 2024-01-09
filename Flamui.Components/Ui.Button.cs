using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public static partial class Ui
{
    public static bool Button(string text, bool primary = false, float width = 70, bool focusable = true, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var btn, key, path, line).Height(23).Width(width).Rounded(2).Focusable(focusable);

            if (primary)
            {
                btn.Color(C.Blue).BorderWidth(0);
            }
            else
            {
                btn.BorderWidth(1).BorderColor(C.Border).Color(C.Transparent);
            }

            if (btn.HasFocusWithin)
            {
                btn.BorderColor(C.Blue).BorderWidth(2);
            }

            Text(text).VAlign(TextAlign.Center).HAlign(TextAlign.Center).Color(C.Text);
        DivEnd();

        return btn.IsClicked;
    }
}
