using System.Runtime.CompilerServices;
using Flamui.UiElements;

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
        using (var hitBox = ui.Div().ShrinkHeight().Color(C.Transparent))
        {
            var t = ui.Text(text);

            if (hitBox.IsClicked)
            {
                var l = t.TextLayoutInfo.Lines[0];

                var pos = ui.Tree.MousePosition - (t.FinalOnScreenSize.GetPosition() + l.Bounds.GetPosition());

                if (pos.X < 0)
                {
                    t.SelectionStart = t.CursorPosition = 0;
                }else if (pos.X > l.Bounds.W)
                {
                    t.SelectionStart = t.CursorPosition = text.Length;
                }
                else
                {
                    var offsets = l.CharOffsets;

                    for (int i = 0; i < offsets.Length; i++)
                    {
                        if (offsets[i] > pos.X)
                        {
                            if (offsets.ContainsIndex(i - 1))
                            {
                                if (Math.Abs(offsets[i - 1] - pos.X) < Math.Abs(offsets[i] - pos.X))
                                {
                                    t.SelectionStart = t.CursorPosition = i;
                                    break;
                                }
                            }

                            t.SelectionStart = t.CursorPosition = i + 1;
                            break;
                        }
                    }
                }
            }

            t.ShowCursor = hasFocus;

            if (hasFocus)
            {
                //todo, if multiple inputs happen in the same frame, stuff breaks, this hole thing should be handled differently
                text = TextBoxInputHandler.ProcessInput(text, t.TextLayoutInfo, ui.Tree.Input, t.UiTextInfo.Multiline, ref t.CursorPosition, ref t.SelectionStart);
            }

            t.UiTextInfo.Content = text;

            return t;
        }
    }
}
