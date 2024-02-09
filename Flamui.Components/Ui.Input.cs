using System.Runtime.CompilerServices;
using Flamui.UiElements;
using SDL2;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static UiText Input(this Flamui.Ui ui, ref string text, bool hasFocus = false, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        ui.DivStart(key, path, line);

        if (hasFocus)
        {
            var input = ui.Window.TextInput;

            if (!string.IsNullOrEmpty(input))
                text += ui.Window.TextInput;

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

        var txt = ui.Text(text).VAlign(TextAlign.Center).Color(C.Text);

        ui.DivEnd();

        return txt;
    }
}
