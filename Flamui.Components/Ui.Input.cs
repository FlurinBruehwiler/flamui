using System.Runtime.CompilerServices;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Input;

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
        var t = ui.Text(text, key, path, line);
        t.ShowCursor = hasFocus;

        if (hasFocus)
        {
            if (ui.Window.IsKeyPressed(Key.Left))
            {
                t.CursorPosition = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, ui.Window.IsKeyDown(Key.ControlLeft) ? MoveType.Word : MoveType.Single);
            }
            if (ui.Window.IsKeyPressed(Key.Right))
            {
                t.CursorPosition = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, ui.Window.IsKeyDown(Key.ControlLeft) ? MoveType.Word : MoveType.Single);
            }

            var input = ui.Window.TextInput;

            if (InputIsValid(input, inputType))
            {
                var (before, after) = SplitCursor(text, t.CursorPosition);
                text = string.Concat(before, input, after);
                t.CursorPosition += input.Length;
            }

            if (ui.Window.IsKeyPressed(Key.Backspace) || ui.Window.IsKeyPressed(Key.Delete))
            {
                var (before, after) = SplitCursor(text, t.CursorPosition);

                var b = before.Span;
                var beforeLen = b.Length;

                HandleBackspace(ui, ref b);

                text = string.Concat(b, after.Span);
                t.CursorPosition -= (beforeLen - b.Length);
            }
        }

        t.UiTextInfo.Content = text;

        return t;
    }

    enum MoveType
    {
        Single,
        Word,
        Line
    }

    private static int GetNewTextPosition(int cursorOffset, TextLayoutInfo layoutInfo, int direction, MoveType moveType)
    {
        if (layoutInfo.Content.Length == 0)
            return cursorOffset;

        if (moveType == MoveType.Single)
        {
            if (direction == +1 && cursorOffset < layoutInfo.Content.Length)
            {
                return cursorOffset + 1;
            }

            if (direction == -1 && cursorOffset > 0)
            {
                return cursorOffset - 1;
            }
        }
        else if (moveType == MoveType.Word)
        {
            bool hasEncounteredNonWhitespace = false;

            while (true)
            {
                var nextOffset = cursorOffset + direction;
                if (nextOffset < 0 || nextOffset >= layoutInfo.Content.Length)
                {
                    if (direction == +1 && nextOffset <= layoutInfo.Content.Length) cursorOffset += 1;

                    return cursorOffset;
                }

                if (char.IsWhiteSpace(layoutInfo.Content[nextOffset]))
                {
                    if (hasEncounteredNonWhitespace)
                    {
                        if (direction == +1) cursorOffset += 1;

                        return cursorOffset;
                    }
                }
                else
                {
                    hasEncounteredNonWhitespace = true;
                }

                cursorOffset = nextOffset;
            }
        }

        return cursorOffset;
    }

    private static int TextPositionToTextOffset(TextPosition cursor, TextLayoutInfo layoutInfo)
    {
        int offset = 0;

        for (var i = 0; i < layoutInfo.Lines.Length; i++)
        {
            var line = layoutInfo.Lines[i];

            if (i == cursor.Line)
            {
                return offset + cursor.Character;
            }

            offset += line.TextContent.Length;
        }

        throw new Exception("Cursor outside of text :(");
    }

    private static (ReadOnlyMemory<char> before, ReadOnlyMemory<char> after) SplitCursor(string text, int cursor)
    {
        return (text.AsMemory(0, cursor), text.AsMemory(cursor));
    }

    private static bool InputIsValid(string text, InputType inputType)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        if (inputType == InputType.Numeric && !int.TryParse(text, out _))
        {
            return false;
        }

        return true;
    }

    private static void HandleBackspace(Ui ui, ref ReadOnlySpan<char> text)
    {
        if (ui.Window.IsKeyDown(Key.ControlLeft))
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
            if(text.Length != 0)
                text = text[..^1];
        }
    }
}
