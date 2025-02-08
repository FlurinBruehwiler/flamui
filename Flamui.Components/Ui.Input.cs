using System.Runtime.CompilerServices;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.GLFW;
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
                var (before, after) = SplitCursor(text, ref t.TextLayoutInfo, t.CursorPosition);
                text = string.Concat(before, input, after);
                t.CursorPosition = new TextPosition(t.CursorPosition.Line, t.CursorPosition.Character + input.Length);
            }

            if (ui.Window.IsKeyPressed(Key.Backspace))
            {
                var (before, after) = SplitCursor(text, ref t.TextLayoutInfo, t.CursorPosition);

                var b = before.Span;
                var beforeLen = b.Length;

                HandleBackspace(ui, ref b);

                text = string.Concat(b, after.Span);
                t.CursorPosition = new TextPosition(t.CursorPosition.Line, t.CursorPosition.Character - (beforeLen - b.Length));
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

    private static TextPosition GetNewTextPosition(TextPosition cursor, TextLayoutInfo layoutInfo, int direction, MoveType moveType)
    {
        if (layoutInfo.Content.Length == 0)
            return cursor;

        for (var i = 0; i < layoutInfo.Lines.Length; i++)
        {
            var line = layoutInfo.Lines[i];

            if (i == cursor.Line)
            {
                //todo goto next/prev line
                if (direction == +1 && cursor.Character < line.TextContent.Length)
                {
                    return cursor with { Character = cursor.Character + 1 };
                }

                if (direction == -1 && cursor.Character > 0)
                {
                    return new TextPosition(cursor.Line, cursor.Character - 1);
                }

                break;
            }
        }

        return cursor;
    }

    private static (ReadOnlyMemory<char> before, ReadOnlyMemory<char> after) SplitCursor(string text, ref TextLayoutInfo layoutInfo, TextPosition cursor)
    {
        if (text.Length == 0)
            return (new ReadOnlyMemory<char>(), new ReadOnlyMemory<char>());

        var beforeCount = 0;
        for (var i = 0; i < layoutInfo.Lines.Length; i++)
        {
            if (i == cursor.Line)
            {
                beforeCount += cursor.Character;
                break;
            }

            beforeCount += layoutInfo.Lines[i].TextContent.Length;
        }

        return (text.AsMemory(0, beforeCount), text.AsMemory(beforeCount));
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
