using System.Runtime.CompilerServices;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void Checkbox(this Ui ui, ref bool enabled, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = ui.CreateIdScope(file, lineNumber);
        using (var div = ui.Rect().Height(15).Focusable().Width(15).Color(ColorPalette.BackgroundColor)
                   .BorderColor(ColorPalette.BorderColor).BorderWidth(1).Rounded(2))
        {
            if (div.IsClicked())
            {
                enabled = !enabled;
            }

            if (div.HasFocusWithin)
            {
                div.BorderColor(ColorPalette.AccentColor).BorderWidth(2);

                if (ui.Tree.IsKeyPressed(Key.Space) ||
                    ui.Tree.IsKeyPressed(Key.Enter))
                {
                    enabled = !enabled;
                }
            }

            if (enabled)
            {
                div.Color(ColorPalette.AccentColor);
                div.BorderWidth(0);
                ui.SvgImage("check").Color(C.White);
            }
        }
    }
}