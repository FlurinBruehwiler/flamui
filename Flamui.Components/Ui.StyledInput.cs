using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static UiContainer StyledInput(this Ui ui, ref string text, string placeholder = "", string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        ui.DivStart(out var modalInputDiv, key, path, line).Focusable().Rounded(2).PaddingHorizontal(5).Height(25).BorderWidth(1).BorderColor(C.Border).Color(C.Transparent);
            if (modalInputDiv.HasFocusWithin)
            {
                modalInputDiv.BorderColor(C.Blue).BorderWidth(2);
                ui.Input(ref text, true);
            }
            else
            {
                var uiText = ui.Text(string.IsNullOrEmpty(text) ? placeholder : text).VAlign(TextAlign.Center).Color(C.Text);
                if (string.IsNullOrEmpty(text))
                {
                    uiText.Color(C.Border);
                }
            }
            ui.DivEnd();

        return modalInputDiv;
    }
}
