using System.Runtime.CompilerServices;
using ImSharpUISample.UiElements;
using SDL2;

namespace ImSharpUISample;

public partial class Ui
{
    public static Stack<ModalComponent> OpenModalComponents = new();

    public static void StartModal(ref bool show, string title, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiElementId(key, path, line);
        if (!OpenElementStack.Peek().OldDataById.TryGetValue(id, out var existingModal))
        {
            existingModal = Activator.CreateInstance<ModalComponent>();
        }

        OpenElementStack.Peek().Data.Add(existingModal);

        var modal = (ModalComponent)existingModal;

        OpenModalComponents.Push(modal);

        modal.StartModal(ref show, title);
    }

    public static void EndModal(ref bool show)
    {
        var modal = OpenModalComponents.Pop();
        modal.EndModal(ref show);
    }

    public static void StyledInput(ref string text, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var modalInputDiv).PaddingHorizontal(5).Height(25).BorderWidth(1).BorderColor(100, 100, 100).Color(0, 0, 0, 0);
            if (modalInputDiv.HasFocusWithin)
                modalInputDiv.BorderColor(53, 116, 240).BorderWidth(2);
            Input(ref text);
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
