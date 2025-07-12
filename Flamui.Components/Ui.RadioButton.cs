using System.Runtime.CompilerServices;
using Silk.NET.Input;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void RadioButton<T>(this Ui ui, ref T selectedValue, T thisValue, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = ui.CreateIdScope(file, lineNumber);

        using (var div = ui.Rect().Circle(8).Focusable().Color(ColorPalette.BackgroundColor).Border(1, ColorPalette.BorderColor).Center())
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

                using (ui.Rect().Circle(3).Color(C.White))
                {

                }
            }
        }
    }
}