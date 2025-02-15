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
            if (ui.Window.IsKeyPressed(Key.Backspace))
            {
                var prevPos = t.CursorPosition;
                t.CursorPosition = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, ui.Window.IsKeyDown(Key.ControlLeft) ? MoveType.Word : MoveType.Single);
                text = string.Concat(text.AsSpan(0, t.CursorPosition), text.AsSpan(prevPos));
            }
            if (ui.Window.IsKeyPressed(Key.Delete))
            {
                var prevPos = t.CursorPosition;
                var deleteTo = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, ui.Window.IsKeyDown(Key.ControlLeft) ? MoveType.Word : MoveType.Single);
                text = string.Concat(text.AsSpan(0, prevPos), text.AsSpan(deleteTo));
            }
            if (ui.Window.IsKeyPressed(Key.V) && ui.Window.IsKeyDown(Key.ControlLeft))
            {
                var (before, after) = SplitCursor(text, t.CursorPosition);
                text = string.Concat(before, ui.Window.Input.ClipboardText, after);
                t.CursorPosition += ui.Window.Input.ClipboardText.Length;
            }

            var input = ui.Window.TextInput;

            if (InputIsValid(input, inputType))
            {
                var (before, after) = SplitCursor(text, t.CursorPosition);
                text = string.Concat(before, input, after);
                t.CursorPosition += input.Length;
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
}
