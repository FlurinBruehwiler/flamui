using System.Runtime.CompilerServices;
using Silk.NET.Input;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void Checkbox(this Ui ui, ref bool enabled, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        using (var div = ui.Div(key, path, line).Height(15).Focusable().Width(15).Color(ColorPalette.BackgroundColor)
                   .BorderColor(ColorPalette.BorderColor).BorderWidth(1).Rounded(2))
        {
            if (div.IsClicked)
            {
                enabled = !enabled;
            }

            if (div.HasFocusWithin)
            {
                div.BorderColor(ColorPalette.AccentColor).BorderWidth(2);

                if (ui.Window.IsKeyPressed(Key.Space) ||
                    ui.Window.IsKeyPressed(Key.Enter))
                {
                    enabled = !enabled;
                }
            }

            if (enabled)
            {
                div.Color(ColorPalette.AccentColor);
                div.BorderWidth(0);
                ui.SvgImage("./Icons/check.svg");
            }
        }
    }
}
