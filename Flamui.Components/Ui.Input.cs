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

                t.CursorPosition = TextBoxInputHandler.GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, moveType);
            }
            if (ui.Window.IsKeyPressed(Key.Right))
            {
                if (!ui.Window.IsKeyDown(Key.ShiftLeft))
                    selectionDisable = true;

                t.CursorPosition = TextBoxInputHandler.GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, moveType);
            }
            if (ui.Window.IsKeyPressed(Key.Home))
            {
                if (!ui.Window.IsKeyDown(Key.ShiftLeft))
                    selectionDisable = true;

                t.CursorPosition = TextBoxInputHandler.GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, MoveType.Line);
            }
            if (ui.Window.IsKeyPressed(Key.End))
            {
                if (!ui.Window.IsKeyDown(Key.ShiftLeft))
                    selectionDisable = true;

                t.CursorPosition = TextBoxInputHandler.GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, MoveType.Line);
            }

            if (ui.Window.IsKeyPressed(Key.Backspace))
            {
                selectionDisable = true;

                var prevPos = t.CursorPosition;
                t.CursorPosition = TextBoxInputHandler.GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, -1, moveType);
                text = string.Concat(text.AsSpan(0, t.CursorPosition), text.AsSpan(prevPos));
            }
            if (ui.Window.IsKeyPressed(Key.Delete))
            {
                selectionDisable = true;

                var prevPos = t.CursorPosition;
                var deleteTo = TextBoxInputHandler.GetNewTextPosition(t.CursorPosition, t.TextLayoutInfo, +1, moveType);
                text = string.Concat(text.AsSpan(0, prevPos), text.AsSpan(deleteTo));
            }
            if (ui.Window.IsKeyPressed(Key.V) && ui.Window.IsKeyDown(Key.ControlLeft))
            {
                selectionDisable = true;

                var (before, after, cursorShift) = TextBoxInputHandler.SplitCursor(text, t.CursorPosition, t.SelectionStart);
                text = string.Concat(before, ui.Window.Input.ClipboardText, after);
                t.CursorPosition += ui.Window.Input.ClipboardText.Length - cursorShift;
            }
            if (ui.Window.IsKeyPressed(Key.C) && ui.Window.IsKeyDown(Key.ControlLeft))
            {
                var range = TextBoxInputHandler.GetSelectedRange(t.CursorPosition, t.SelectionStart);
                if (range.Start.Value != range.End.Value)
                {
                    ui.Window.Input.ClipboardText = text[range];
                }
            }
            if (ui.Window.IsKeyPressed(Key.X) && ui.Window.IsKeyDown(Key.ControlLeft))
            {
                var range = TextBoxInputHandler.GetSelectedRange(t.CursorPosition, t.SelectionStart);
                if (range.Start.Value != range.End.Value)
                {
                    ui.Window.Input.ClipboardText = text[range];
                }
            }

            var input = ui.Window.TextInput;

            if (InputIsValid(input, inputType))
            {
                selectionDisable = true;

                var (before, after, cursorShift) = TextBoxInputHandler.SplitCursor(text, t.CursorPosition, t.SelectionStart);
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

    public static bool InputIsValid(string text, InputType inputType)
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
