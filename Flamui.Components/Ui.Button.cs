using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static bool Button(this Ui ui, string text, bool primary = false, float width = 70, bool focusable = true, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        using (var btn = ui.Div(key, path, line).Height(23).Rounded(2).Focusable(focusable).ShrinkWidth().Direction(Dir.Horizontal).PaddingHorizontal(10).XAlign(XAlign.Center))
        {
            if (primary)
            {
                btn.Color(ColorPalette.AccentColor).BorderWidth(0);
            }
            else
            {
                btn.BorderWidth(1).BorderColor(ColorPalette.BorderColor).Color(C.Transparent);
            }

            if (btn.HasFocusWithin)
            {
                btn.BorderColor(ColorPalette.AccentColor).BorderWidth(2);
            }

            ui.Text(text).Color(ColorPalette.TextColor);

            return btn.IsClicked;
        }
    }
}
