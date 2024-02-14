using System.Runtime.CompilerServices;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static bool SquareButton(this Ui ui, string icon, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        using (ui.Div(out var btn, key, path, line).Height(30).Width(30).Rounded(2).Focusable().Color(C.Transparent))
        {
            if (btn.HasFocusWithin)
            {
                btn.BorderColor(C.Blue).BorderWidth(2);
            }

            ui.SvgImage(icon, C.Text);

            return btn.IsClicked;

        }
    }
}
