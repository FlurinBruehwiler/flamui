using System.Runtime.CompilerServices;
using Flamui.UiElements;
using SDL2;

namespace Flamui.Components;

public enum InputType
{
    Text,
    Numeric
}

public static partial class UiExtensions
{
    public static UiText Input(this Ui ui, ref string text, bool hasFocus = false, InputType inputType = InputType.Text, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        using (ui.Div(key, path, line).MAlign(MAlign.Center))
        {
            if (hasFocus)
            {
                var input = ui.Window.TextInput;

                if (InputIsValid(input, inputType))
                    text += input;

                HandleBackspace(ui, ref text);
            }

            return ui.Text(text).Color(ColorPalette.TextColor);
        }
    }

    private static bool InputIsValid(string text, InputType inputType)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        if (inputType == InputType.Numeric && !int.TryParse(text, out _))
        {
            return false;
        }

        return true;
    }

    private static void HandleBackspace(Ui ui, ref string text)
    {
        if (ui.Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE))
        {
            if (ui.Window.IsKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_LCTRL))
            {
                text = text.TrimEnd();

                if (!text.Contains(' '))
                {
                    text = string.Empty;
                }

                for (var i = text.Length - 1; i > 0; i--)
                {
                    if (text[i] != ' ') continue;
                    text = text[..(i + 1)];
                    break;
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(text))
                    text = text[..^1];
            }
        }
    }
}
