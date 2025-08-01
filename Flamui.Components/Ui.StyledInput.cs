﻿using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public static partial class UiExtensions
{
    //todo: make placeholder work again :)
    public static FlexContainer StyledInput(this Ui ui, ref string text, bool multiline = false, string placeholder = "", InputType inputType = InputType.Text, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = ui.CreateIdScope(file, lineNumber);
        using (var modalInputDiv = ui.Rect().Focusable().Rounded(2).PaddingHorizontal(5).ShrinkHeight(25)
                   .BorderWidth(1).BorderColor(ColorPalette.BorderColor).Color(ColorPalette.BackgroundColor).MainAlign(MAlign.Center).Clip())
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
