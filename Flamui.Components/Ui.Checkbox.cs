using System.Runtime.CompilerServices;
using SDL2;

namespace Flamui.Components;

public static partial class Ui
{
    public static void Checkbox(ref bool enabled, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var div, key, path, line).Height(15).Focusable().Width(15).Color(C.Background).BorderColor(C.Border).BorderWidth(1).Rounded(2);
        if (div.IsClicked)
        {
            enabled = !enabled;
        }

        if (div.HasFocusWithin)
        {
            div.BorderColor(C.Blue).BorderWidth(2);

            if (Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_SPACE) ||
                Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
            {
                enabled = !enabled;
            }
        }

        if (enabled)
        {
            div.Color(C.Blue);
            div.BorderWidth(0);
            SvgImage("./Icons/check.svg");
        }

        DivEnd();
    }
}
