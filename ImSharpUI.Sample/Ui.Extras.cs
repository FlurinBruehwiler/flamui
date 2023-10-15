using System.Runtime.CompilerServices;
using ImSharpUISample.UiElements;
using SDL2;

namespace ImSharpUISample;

public partial class Ui
{
    private static bool _wasShown;

    public static void StartModal(ref bool show, string title)
    {
        // Console.WriteLine(show);
        DivStart().Absolute(Root).XAlign(XAlign.Center).MAlign(MAlign.Center).ZIndex(1).Hidden(!show);
            DivStart(out var modalDiv).Clip().Color(39, 41, 44).Width(400).Height(200).Radius(10).BorderWidth(2).BorderColor(58, 62, 67);

                if (_wasShown && TryGetMouseClickPosition(out var pos) && !modalDiv.ContainsPoint(pos.X, pos.Y))
                    show = false;

                _wasShown = show;

                //Header
                DivStart().Height(25).Dir(Dir.Horizontal).MAlign(MAlign.SpaceBetween).PaddingLeft(10);
                    //Title
                    DivStart();
                        Text(title).VAlign(TextAlign.Center);
                    DivEnd();

                    //Close button
                    DivStart(out var closeButton).Width(25);
                        if (closeButton.Clicked)
                            show = false;
                        SvgImage("close.svg");
                    DivEnd();
                DivEnd();

                //Main Content
                DivStart().Padding(15);

                    //start user content
                    DivStart();
    }

    public static void EndModal()
    {
                    //End user content
                    DivEnd();

                    //Footer
                    DivStart().Height(25).Dir(Dir.Horizontal).MAlign(MAlign.FlexEnd).Gap(10);
                        DivStart().Width(70).Color(53, 116, 240).Radius(3);
                            Text("Next").VAlign(TextAlign.Center).HAlign(TextAlign.Center).Color(230, 230, 230);
                        DivEnd();
                        DivStart().Width(70).Color(43, 45, 48).Radius(3).BorderWidth(1).BorderColor(100, 100, 100);
                            Text("Cancel").VAlign(TextAlign.Center).HAlign(TextAlign.Center).Color(230, 230, 230);;
                        DivEnd();
                    DivEnd();

                DivEnd();
            DivEnd();
        DivEnd();
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
