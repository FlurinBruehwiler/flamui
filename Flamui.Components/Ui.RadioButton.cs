using Silk.NET.Input;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void RadioButton<T>(this Ui ui, ref T selectedValue, T thisValue)
    {
        using (var div = ui.Rect().Height(16).Focusable().Width(16).Color(ColorPalette.BackgroundColor)
                   .BorderColor(ColorPalette.BorderColor).BorderWidth(1).Rounded(8))
        {
            if (div.IsClicked)
            {
                selectedValue = thisValue;
            }

            if (div.HasFocusWithin)
            {
                div.BorderColor(ColorPalette.AccentColor).BorderWidth(2);

                if (ui.Tree.IsKeyPressed(Key.Space) ||
                    ui.Tree.IsKeyPressed(Key.Enter))
                {
                    selectedValue = thisValue;
                }
            }

            if (EqualityComparer<T>.Default.Equals(selectedValue, thisValue))
            {
                div.Color(ColorPalette.AccentColor);
                div.BorderWidth(0);
            }
        }
    }
}