using System.Runtime.CompilerServices;

namespace Flamui.Components;

public static partial class Ui
{
    public static bool SquareButton(string icon, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var btn, key, path, line).Height(30).Width(30).Rounded(2).Focusable().Color(C.Transparent);

            if (btn.HasFocusWithin)
            {
                btn.BorderColor(C.Blue).BorderWidth(2);
            }

            SvgImage(icon, C.Text);

        DivEnd();

        return btn.IsClicked;
    }
}
