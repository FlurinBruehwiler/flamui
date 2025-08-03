using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui.Components;

public class InputValidation
{
    public static Func<string, bool> IsInteger() => static str => int.TryParse(str, out _);
    public static Func<string, bool> IsFloat() => static str => float.TryParse(str, out _);

    public List<Func<string, bool>> Validations = [];
    public bool _isValid;

    public void Invalidate(ref bool hasErrors)
    {
        if (!_isValid)
            hasErrors = true;
    }

    public InputValidation WithValidation(Func<string, bool> validationFunc)
    {
        Validations.Add(validationFunc);
        return this;
    }
}

public static partial class UiExtensions
{
    //todo: make placeholder work again :)
    public static InputValidation StyledInput(this Ui ui, ref string text, bool multiline = false, string placeholder = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        //todo make a validation system, to check if the input type is correct

        var inputValidation = ui.GetObj<InputValidation>();
        inputValidation._isValid = true;

        using var _ = ui.CreateIdScope(file, lineNumber);
        using (var modalInputDiv = ui.Rect().Focusable().Rounded(2).PaddingHorizontal(5).ShrinkHeight(25)
                   .BorderWidth(1).BorderColor(ColorPalette.BorderColor).Color(ColorPalette.BackgroundColor).MainAlign(MAlign.Center).Clip())
        {
            if (modalInputDiv.HasFocusWithin)
            {
                modalInputDiv.BorderColor(ColorPalette.AccentColor).BorderWidth(2);
            }

            ui.Input(ref text, modalInputDiv.HasFocusWithin).Multiline(multiline);


            foreach (var validation in inputValidation.Validations)
            {
                if (!validation(text))
                {
                    modalInputDiv.Border(2, C.Red7);
                    inputValidation._isValid = false;
                    break;
                }
            }
        }

        inputValidation.Validations.Clear();

        return inputValidation;
    }
}
