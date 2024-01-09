using System.Runtime.CompilerServices;
using Flamui.UiElements;
using SDL2;

namespace Flamui.Components;

public static partial class Ui
{
    public static UiText Input(ref string text, bool hasFocus = false, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(key, path, line);

        if (hasFocus)
        {
            var input = Window.TextInput;

            if (!string.IsNullOrEmpty(input))
                text += Window.TextInput;

            if (Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE))
            {
                if (Window.IsKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_LCTRL))
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

        var txt = Text(text).VAlign(TextAlign.Center).Color(C.Text);

        DivEnd();

        return txt;
    }
}
