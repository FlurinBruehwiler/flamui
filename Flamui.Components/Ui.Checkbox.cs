using Silk.NET.Input;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void Checkbox(this Ui ui, ref bool enabled)
    {
        using (var div = ui.Div().Height(15).Focusable().Width(15).Color(ColorPalette.BackgroundColor)
                   .BorderColor(ColorPalette.BorderColor).BorderWidth(1).Rounded(2))
        {
            if (div.IsClicked)
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
                ui.SvgImage("./Icons/check.svg");
            }
        }
    }
}
