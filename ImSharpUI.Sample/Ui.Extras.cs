using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using ImSharpUISample.UiElements;
using SDL2;

namespace ImSharpUISample;

public class ComponentData : IData
{
    public required UiElementId Id { get; set; }
    public required object Component { get; set; }
    public required SubStack SubStack { get; set; }
}

public partial class Ui
{
    public static UiContainer substackroot;

    // public static void StartModal(string key = "",
    //     [CallerFilePath] string path = "",
    //     [CallerLineNumber] int line = -1)
    // {
    //     var id = new UiElementId(key, path, line);
    //     if (!OpenElementStack.Peek().OldDataById.TryGetValue(id, out var existingModal))
    //     {
    //         existingModal = new ComponentData
    //         {
    //             Component = Activator.CreateInstance<ModalComponent>(),
    //             Id = id,
    //             SubStack = new SubStack
    //             {
    //                 PreviousSubStack = OpenElementStack,
    //                 CurrentStack = new Stack<UiContainer>()
    //             }
    //         };
    //         substackroot = new UiContainer
    //         {
    //             Id = new UiElementId("substack root", string.Empty, 0)
    //         };
    //         ((ComponentData)existingModal).SubStack.CurrentStack.Push(substackroot);
    //     }
    //
    //     OpenElementStack.Peek().Data.Add(existingModal);
    //
    //     var componentData = (ComponentData)existingModal;
    //     OpenComponents.Push(componentData);
    //     var modal = (ModalComponent)componentData.Component;
    //     componentData.SubStack.CurrentStack.Peek().OpenElement();
    //     modal.StartModal();
    //
    //     OpenElementStack = componentData.SubStack.CurrentStack;
    // }
    //
    // public static void EndModal(ref bool show, string title)
    // {
    //     var componentData = OpenComponents.Pop();
    //     OpenElementStack = componentData.SubStack.PreviousSubStack;
    //     var children = componentData.SubStack.CurrentStack.Peek().Children;
    //     ((ModalComponent)componentData.Component).EndModal(ref show, title, children);
    // }

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

            var input = Window.TextInput;

            if (!string.IsNullOrEmpty(input) && inputDiv.IsActive)
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
                    text = text[..^1];
                }
            }

            Text(text).VAlign(TextAlign.Center).Color(200, 200, 200);

        DivEnd();
    }
}
