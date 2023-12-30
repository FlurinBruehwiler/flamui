using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public static partial class Ui
{
    public static UiContainer StyledInput(ref string text, string placeholder = "", string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var modalInputDiv, key, path, line).Focusable().Rounded(2).PaddingHorizontal(5).Height(25).BorderWidth(1).BorderColor(C.Border).Color(C.Transparent);
            if (modalInputDiv.HasFocusWithin)
            {
                modalInputDiv.BorderColor(C.Blue).BorderWidth(2);
                Input(ref text, true);
            }
            else
            {
                var uiText = Text(string.IsNullOrEmpty(text) ? placeholder : text).VAlign(TextAlign.Center).Color(C.Text);
                if (string.IsNullOrEmpty(text))
                {
                    uiText.Color(C.Border);
                }
            }
        DivEnd();

        return modalInputDiv;
    }
}
