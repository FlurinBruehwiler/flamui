namespace Flamui.Components;

public static partial class UiExtensions
{
    public static bool SquareButton(this Ui ui, string icon)
    {
        using (var btn = ui.Rect().Height(20).Width(20).Rounded(2).Focusable().Color(C.Transparent).Padding(1))
        {
            if (btn.HasFocusWithin)
            {
                btn.BorderColor(ColorPalette.AccentColor).BorderWidth(2);
            }

            if (btn.IsHovered)
            {
                btn.Color(new ColorDefinition(0, 0, 0, 100));
            }

            ui.SvgImage(icon).Color(ColorPalette.TextColor);

            return btn.IsClicked();

        }
    }
}
