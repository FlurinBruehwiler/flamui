using System.Runtime.CompilerServices;
using Flamui.UiElements;
using SDL2;

namespace Flamui;



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

    public static void Checkbox(ref bool enabled, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var div, key, path, line).Height(15).Focusable().Width(15).Color(C.Background).BorderColor(C.Border).BorderWidth(1).Rounded(2);
            if (div.Clicked)
            {
                enabled = !enabled;
            }

            if (div.HasFocusWithin)
            {
                div.BorderColor(C.Blue).BorderWidth(2);
            }

            if (enabled)
            {
                div.Color(C.Blue);
                div.BorderWidth(0);
                SvgImage("./Icons/check.svg");
            }

        DivEnd();
    }

    public static bool Button(string text, bool primary = false, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var btn, key, path, line).Height(23).Width(70).Rounded(2).Focusable();

            if (primary)
            {
                btn.Color(C.Blue).BorderWidth(0);
            }
            else
            {
                btn.BorderWidth(1).BorderColor(C.Border).Color(C.Transparent);
            }

            if (btn.HasFocusWithin)
            {
                btn.BorderColor(C.Blue).BorderWidth(2);
            }

            Text(text).VAlign(TextAlign.Center).HAlign(TextAlign.Center);
        DivEnd();

        return btn.Clicked;
    }

    public static UiContainer StyledInput(ref string text, string placeholder = "", string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        DivStart(out var modalInputDiv, key, path, line).Focusable().Rounded(2).PaddingHorizontal(5).Height(25).BorderWidth(1).BorderColor(C.Border).Color(C.Transparent);
            if (modalInputDiv.HasFocusWithin)
            {
                modalInputDiv.BorderColor(C.Blue).BorderWidth(2);
                Input(ref text);
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

    public static void Input(ref string text, bool hasFocus = false, string key = "",
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

            Text(text).VAlign(TextAlign.Center).Color(C.Text);

        DivEnd();
    }
}
