using System.Runtime.CompilerServices;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static bool SquareButton(this Ui ui, string icon, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        using (var btn = ui.Div(key, path, line).Height(30).Width(30).Rounded(2).Focusable().Color(C.Transparent))
        {
            if (btn.HasFocusWithin)
            {
                btn.BorderColor(ColorPalette.AccentColor).BorderWidth(2);
            }

            ui.SvgImage(icon, ColorPalette.TextColor);

            return btn.IsClicked;

        }
    }
}
