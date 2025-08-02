using System.Diagnostics;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.GLFW;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui.Components;

public enum InputType
{
    Text,
    Numeric
}

public static partial class UiExtensions
{
    private static int GetCharacterUnderMouse(Ui ui, UiText t, string text)
    {
        var l = t.TextLayoutInfo.Lines[0];

        var pos = ui.Tree.MousePosition - (t.FinalOnScreenSize.GetPosition() + l.Bounds.GetPosition());

        if (pos.X < 0)
            return 0;

        if (pos.X > l.Bounds.W)
            return text.Length;

        var offsets = l.CharOffsets;

        for (int i = 0; i < offsets.Length; i++)
        {
            if (offsets[i] > pos.X)
            {
                if (offsets.ContainsIndex(i - 1))
                {
                    if (Math.Abs(offsets[i - 1] - pos.X) < Math.Abs(offsets[i] - pos.X))
                    {
                        return i;
                    }
                }

                return i + 1;
            }
        }

        throw new Exception(":(");
    }

    public static UiText Input(this Ui ui, ref string text, bool hasFocus = false, InputType inputType = InputType.Text)
    {
        if (text == null)
            throw new Exception("text cannot be null");

        ref bool lastClickWasDoubleClick = ref ui.Get(false);

        using (var hitBox = ui.Rect().ShrinkHeight().Color(C.Transparent))
        {
            if (hasFocus)
            {
                hitBox.Clip();
            }

            var t = ui.Text(text).TrimMode(hasFocus ? TextTrimMode.None : TextTrimMode.AddEllipsis);

            if (hitBox.IsHovered)
            {
                ui.Tree.UseCursor(CursorShape.IBeam);
            }

            if (hitBox.IsHovered)
            {
                if (ui.Tree.IsMouseButtonPressed(MouseButton.Left))
                {
                    t.SelectionStart = t.CursorPosition = GetCharacterUnderMouse(ui, t, text);
                }

                if (ui.Tree.IsMouseButtonDown(MouseButton.Left) && !lastClickWasDoubleClick)
                {
                    t.CursorPosition = GetCharacterUnderMouse(ui, t, text);
                }

                if (ui.Tree.IsMouseButtonReleased(MouseButton.Left))
                {
                    lastClickWasDoubleClick = false;
                }

                if (hitBox.IsDoubleClicked())
                {
                    lastClickWasDoubleClick = true;
                    var c = GetCharacterUnderMouse(ui, t, text);

                    (t.SelectionStart, t.CursorPosition) = GetWordUnderCursor(text, c);
                }
            }

            t.ShowCursor = hasFocus;

            if (hasFocus)
            {
                //todo, if multiple inputs happen in the same frame, stuff breaks, this hole thing should be handled differently
                text = TextBoxInputHandler.ProcessInput(text, t.TextLayoutInfo, ui.Tree, t.UiTextInfo.Multiline, ref t.CursorPosition, ref t.SelectionStart);
            }

            t.UiTextInfo.Content = text;

            return t;
        }
    }

    public static (int start, int end) GetWordUnderCursor(string text, int cursor)
    {
        var wordStart = text.AsSpan().Slice(0, cursor).LastIndexOfAny([' ', '\t', '\n']);
        wordStart = wordStart == -1 ? 0 : wordStart + 1;

        var wordEnd = text.AsSpan().Slice(cursor).IndexOfAny([' ', '\t', '\n']);
        wordEnd = wordEnd == -1 ? text.Length : wordEnd + cursor;

        return (wordStart, wordEnd);
    }
}
