using Flamui.Drawing;
using Silk.NET.Input;

namespace Flamui;

public enum MoveType
{
    Single,
    Word,
    Line
}

public static class TextBoxInputHandler
{
    public static string ProcessInput(TextLayoutInfo textLayout, Input input, ref int cursorPosition, ref int selectionStart)
    {
        string text = textLayout.Content.ToString();

        bool selectionDisable = false;

        if (input.KeyPressed.Contains(Key.ShiftLeft))
        {
            selectionStart = cursorPosition;
        }

        var moveType = input.KeyDown.Contains(Key.ControlLeft) ? MoveType.Word : MoveType.Single;

        if (input.KeyPressed.Contains(Key.Left))
        {
            if (!input.KeyDown.Contains(Key.ShiftLeft))
                selectionDisable = true;

            cursorPosition = GetNewTextPosition(cursorPosition, textLayout, -1, moveType);
        }
        if (input.KeyPressed.Contains(Key.Right))
        {
            if (!input.KeyDown.Contains(Key.ShiftLeft))
                selectionDisable = true;

            cursorPosition = GetNewTextPosition(cursorPosition, textLayout, +1, moveType);
        }
        if (input.KeyPressed.Contains(Key.Home))
        {
            if (!input.KeyDown.Contains(Key.ShiftLeft))
                selectionDisable = true;

            cursorPosition = GetNewTextPosition(cursorPosition, textLayout, -1, MoveType.Line);
        }
        if (input.KeyPressed.Contains(Key.End))
        {
            if (!input.KeyDown.Contains(Key.ShiftLeft))
                selectionDisable = true;

            cursorPosition = GetNewTextPosition(cursorPosition, textLayout, +1, MoveType.Line);
        }

        if (input.KeyPressed.Contains(Key.Backspace))
        {
            selectionDisable = true;

            var prevPos = cursorPosition;
            cursorPosition = GetNewTextPosition(cursorPosition, textLayout, -1, moveType);
            text = string.Concat(text.AsSpan(0, cursorPosition), text.AsSpan(prevPos));
        }
        if (input.KeyPressed.Contains(Key.Delete))
        {
            selectionDisable = true;

            var prevPos = cursorPosition;
            var deleteTo = GetNewTextPosition(cursorPosition, textLayout, +1, moveType);
            text = string.Concat(text.AsSpan(0, prevPos), text.AsSpan(deleteTo));
        }
        if (input.KeyPressed.Contains(Key.V) && input.KeyDown.Contains(Key.ControlLeft))
        {
            selectionDisable = true;

            var (before, after, cursorShift) = SplitCursor(text, cursorPosition, selectionStart);
            text = string.Concat(before, input.ClipboardText, after);
            cursorPosition += input.ClipboardText.Length - cursorShift;
        }
        if (input.KeyPressed.Contains(Key.C) && input.KeyDown.Contains(Key.ControlLeft))
        {
            var range = GetSelectedRange(cursorPosition, selectionStart);
            if (range.Start.Value != range.End.Value)
            {
                input.ClipboardText = text[range];
            }
        }
        if (input.KeyPressed.Contains(Key.X) && input.KeyDown.Contains(Key.ControlLeft))
        {
            var range = GetSelectedRange(cursorPosition, selectionStart);
            if (range.Start.Value != range.End.Value)
            {
                input.ClipboardText = text[range];
            }
        }

        if (input.TextInput != string.Empty)
        {
            selectionDisable = true;

            var (before, after, cursorShift) = SplitCursor(text, cursorPosition, selectionStart);
            text = string.Concat(before, input.TextInput, after);
            cursorPosition += input.TextInput.Length - cursorShift;
        }

        if (selectionDisable)
        {
            selectionStart = cursorPosition;
        }

        return text;
    }
    
        public static Range GetSelectedRange(int cursorPosition, int selectionStart)
    {
        var start = Math.Min(selectionStart, cursorPosition);
        var end = Math.Max(selectionStart, cursorPosition);
        return new Range(new Index(start), new Index(end));
    }

    public static int GetNewTextPosition(int cursorOffset, TextLayoutInfo layoutInfo, int direction, MoveType moveType)
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
        else if (moveType == MoveType.Line)
        {
            if (direction == +1)
            {
                return layoutInfo.Content.Length;
            }

            if (direction == -1)
            {
                return 0;
            }
        }

        return cursorOffset;
    }

    public static (ReadOnlyMemory<char> before, ReadOnlyMemory<char> after, int cursorShift) SplitCursor(string text, int cursorPosition, int selectionStart)
    {
        var selectionRange = GetSelectedRange(cursorPosition, selectionStart);

        int cursorShift = 0;
        if (selectionStart < cursorPosition)
        {
            cursorShift = selectionRange.End.Value - selectionRange.Start.Value;
        }

        return (text.AsMemory(0, selectionRange.Start.Value), text.AsMemory(selectionRange.End.Value), cursorShift);
    }
}