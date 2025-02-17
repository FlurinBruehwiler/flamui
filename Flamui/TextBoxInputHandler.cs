using System.Diagnostics;
using Flamui.Drawing;
using Silk.NET.Input;

namespace Flamui;

public enum MoveType
{
    Single,
    Word,
    Line
}

public struct MyRange
{
    public MyRange(int start, int end)
    {
        if (start > end) throw new Exception("not allowd!!!");

        Start = start;
        End = end;
    }

    public int Start;
    public int End;

    public int GetLength() => End - Start;

    public Range ToRange()
    {
        return new Range(new Index(Start), new Index(End));
    }
}

public static class TextBoxInputHandler
{
    public static string ProcessInput(string text, TextLayoutInfo textLayout, Input input, bool allowMultiline, ref int cursorPosition, ref int selectionStart)
    {
        Debug.Assert(text.AsSpan().Equals(textLayout.Content.AsSpan(), StringComparison.Ordinal));

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

            if (GetSelectedRange(cursorPosition, selectionStart).GetLength() > 0)
            {
                var (before, after, cursorShift) = SplitCursor(text, cursorPosition, selectionStart);
                text = string.Concat(before, after);
                cursorPosition -= cursorShift;
            }
            else
            {
                var prevPos = cursorPosition;
                cursorPosition = GetNewTextPosition(cursorPosition, textLayout, -1, moveType);
                text = string.Concat(text.AsSpan(0, cursorPosition), text.AsSpan(prevPos));
            }
        }
        if (input.KeyPressed.Contains(Key.Delete))
        {
            selectionDisable = true;

            if (GetSelectedRange(cursorPosition, selectionStart).GetLength() > 0)
            {
                var (before, after, cursorShift) = SplitCursor(text, cursorPosition, selectionStart);
                text = string.Concat(before, after);
                cursorPosition -= cursorShift;
            }
            else
            {
                var prevPos = cursorPosition;
                var deleteTo = GetNewTextPosition(cursorPosition, textLayout, +1, moveType);
                text = string.Concat(text.AsSpan(0, prevPos), text.AsSpan(deleteTo));
            }
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
            if (range.Start != range.End)
            {
                input.ClipboardText = text[range.ToRange()];
            }
        }
        if (input.KeyPressed.Contains(Key.X) && input.KeyDown.Contains(Key.ControlLeft))
        {
            var range = GetSelectedRange(cursorPosition, selectionStart);
            if (range.Start != range.End)
            {
                input.ClipboardText = text[range.ToRange()];
            }
            var (before, after, cursorShift) = SplitCursor(text, cursorPosition, selectionStart);
            text = string.Concat(before, after);
            cursorPosition -= cursorShift;
        }

        if (input.KeyPressed.Contains(Key.A) && input.KeyDown.Contains(Key.ControlLeft))
        {
            selectionStart = 0;
            cursorPosition = text.Length;
        }

        var textInput = input.TextInput;

        if (allowMultiline && input.KeyPressed.Contains(Key.Enter))
            textInput += "\n";

        if (textInput != string.Empty)
        {
            selectionDisable = true;

            var (before, after, cursorShift) = SplitCursor(text, cursorPosition, selectionStart);
            text = string.Concat(before, textInput, after);
            cursorPosition += textInput.Length - cursorShift;
        }

        if (selectionDisable)
        {
            selectionStart = cursorPosition;
        }

        return text;
    }
    
    public static MyRange GetSelectedRange(int cursorPosition, int selectionStart)
    {
        var start = Math.Min(selectionStart, cursorPosition);
        var end = Math.Max(selectionStart, cursorPosition);
        return new MyRange(start, end);
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
        if (cursorPosition > selectionStart)
        {
            cursorPosition -= 1;
        }

        var selectionRange = GetSelectedRange(cursorPosition, selectionStart);

        int cursorShift = 0;
        if (selectionStart < cursorPosition)
        {
            cursorShift = selectionRange.End - selectionRange.Start;
            if (cursorPosition > selectionStart)
            {
                cursorShift++;
            }
        }

        return (text.AsMemory(0, selectionRange.Start), text.AsMemory(selectionRange.End), cursorShift);
    }
}