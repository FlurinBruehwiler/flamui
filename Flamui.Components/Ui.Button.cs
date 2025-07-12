using System.Runtime.CompilerServices;

namespace Flamui.Components;

/*
 * We could create one giant dictionary, with ID => UiElement, but the ID wouldn't just consist of the location in the source code, but also combined with the parent ID.
 */
public static partial  class UiExtensions
{
    public static bool TestFunc(Ui ui)
    {
        // var res = Button(ui, "hi");
        // res = ui.Button("hi");
        // return res;
        return true;
    }

    public static void Anita(this Ui ui)
    {

    }

    public static bool Button(this Ui ui, string text, bool primary = false, bool focusable = true, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = ui.CreateIdScope(file, lineNumber);

        using (var btn = ui.Rect().Height(23).Rounded(2).Focusable(focusable).ShrinkWidth().Direction(Dir.Horizontal).PaddingHorizontal(10).CrossAlign(XAlign.Center))
        {
            if (primary)
            {
                btn.Color(ColorPalette.AccentColor).BorderWidth(0);
            }
            else
            {
                btn.BorderWidth(1).BorderColor(ColorPalette.BorderColor).Color(ColorPalette.BackgroundColor);
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
