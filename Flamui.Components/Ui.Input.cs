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
        //todo, if multiple inputs happen in the same frame, stuff breaks, this hole thing should be handled differently

        var t = ui.Text(text, key, path, line);
        t.ShowCursor = hasFocus;

        if (hasFocus)
        {
            bool selectionDisable = false;

            if (ui.Window.IsKeyPressed(Key.ShiftLeft))
            {
                t.SelectionStart = t.CursorPosition;
            }

            var moveType = ui.Window.IsKeyDown(Key.ControlLeft) ? MoveType.Word : MoveType.Single;

            if (ui.Window.IsKeyPressed(Key.Left))
            {
                if (!ui.Window.IsKeyDown(Key.ShiftLeft))
                    selectionDisable = true;

                t.CursorPosition = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, moveType);
            }
            if (ui.Window.IsKeyPressed(Key.Right))
            {
                if (!ui.Window.IsKeyDown(Key.ShiftLeft))
                    selectionDisable = true;

                t.CursorPosition = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, moveType);
            }
            if (ui.Window.IsKeyPressed(Key.Backspace))
            {
                selectionDisable = true;

                var prevPos = t.CursorPosition;
                t.CursorPosition = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, moveType);
                text = string.Concat(text.AsSpan(0, t.CursorPosition), text.AsSpan(prevPos));
            }
            if (ui.Window.IsKeyPressed(Key.Delete))
            {
                selectionDisable = true;

                var prevPos = t.CursorPosition;
                var deleteTo = GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, moveType);
                text = string.Concat(text.AsSpan(0, prevPos), text.AsSpan(deleteTo));
            }
            if (ui.Window.IsKeyPressed(Key.V) && ui.Window.IsKeyDown(Key.ControlLeft))
            {
                selectionDisable = true;

                var (before, after, cursorShift) = SplitCursor(text, t);
                text = string.Concat(before, ui.Window.Input.ClipboardText, after);
                t.CursorPosition += ui.Window.Input.ClipboardText.Length - cursorShift;
            }
            if (ui.Window.IsKeyPressed(Key.C) && ui.Window.IsKeyDown(Key.ControlLeft))
            {
                var range = GetSelectedRange(t);
                if (range.Start.Value != range.End.Value)
                {
                    ui.Window.Input.ClipboardText = text[range];
                }
            }

            var input = ui.Window.TextInput;

            if (InputIsValid(input, inputType))
            {
                selectionDisable = true;

                var (before, after, cursorShift) = SplitCursor(text, t);
                text = string.Concat(before, input, after);
                t.CursorPosition += input.Length - cursorShift;
            }

            if (selectionDisable)
            {
                t.SelectionStart = t.CursorPosition;
            }
        }

        t.UiTextInfo.Content = text;

        return t;
    }

    private static Range GetSelectedRange(UiText text)
    {
        var start = Math.Min(text.SelectionStart, text.CursorPosition);
        var end = Math.Max(text.SelectionStart, text.CursorPosition);
        return new Range(new Index(start), new Index(end));
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

    private static (ReadOnlyMemory<char> before, ReadOnlyMemory<char> after, int cursorShift) SplitCursor(string text, UiText uiText)
    {
        var selectionRange = GetSelectedRange(uiText);

        int cursorShift = 0;
        if (uiText.SelectionStart < uiText.CursorPosition)
        {
            cursorShift = selectionRange.End.Value - selectionRange.Start.Value;
        }

        return (text.AsMemory(0, selectionRange.Start.Value), text.AsMemory(selectionRange.End.Value), cursorShift);
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
