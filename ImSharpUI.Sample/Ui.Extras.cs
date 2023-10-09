using System.Runtime.CompilerServices;
using ImSharpUISample.UiElements;
using SDL2;

namespace ImSharpUISample;

public partial class Ui
{
    public static void StartModal()
    {
        DivStart().Absolute().XAlign(XAlign.Center).MAlign(MAlign.Center);
            DivStart().Color(200, 0, 0).Width(200).Height(200);
    }

    public static void EndModal()
    {
            DivEnd();
        DivEnd();
    }

    public static void Input(ref string text, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var inputDiv, key, path, line).Focusable();

            var input = GetTextInput();
            if (!string.IsNullOrEmpty(input) && inputDiv.IsActive)
                text += GetTextInput();

            if (IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE))
            {
                if (IsKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_LCTRL))
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
                    text = text[..^1];
                }
            }

            Text(text).VAlign(TextAlign.Center).Color(200, 200, 200);

        DivEnd();
    }
}
