using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public static partial class UiExtensions
{
    //todo: make placeholder work again :)
    public static FlexContainer StyledInput(this Ui ui, ref string text, bool multiline = false, string placeholder = "", InputType inputType = InputType.Text, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        using (var modalInputDiv = ui.Div(key, path, line).Focusable().Rounded(2).PaddingHorizontal(5).ShrinkHeight(25)
                   .BorderWidth(1).BorderColor(ColorPalette.BorderColor).Color(ColorPalette.BackgroundColor).MainAlign(MAlign.Center))
        {
            if (modalInputDiv.HasFocusWithin)
            {
                modalInputDiv.BorderColor(ColorPalette.AccentColor).BorderWidth(2);
            }

            ui.Input(ref text, modalInputDiv.HasFocusWithin, inputType).Multiline(multiline);

            return modalInputDiv;
        }
    }
}
