using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Input;
using StbTrueTypeSharp;

namespace Flamui.Test;

public class TextEditTests
{
    // Legend:
    // |        The cursor
    // </>    The selected text

    [Fact]
    public void TextInputEnd()
    {
        var initialText = "abcdef|";

        var output = PerformTextInput(initialText, "X");

        Assert.Equal("abcdefX|", output);
    }

    [Fact]
    public void TextInputMiddle()
    {
        var initialText = "abc|def";

        var output = PerformTextInput(initialText, "X");

        Assert.Equal("abcX|def", output);
    }

    [Fact]
    public void TextInputSelected()
    {
        var initialText = "a<bc|def";

        var output = PerformTextInput(initialText, "X");

        Assert.Equal("aX|def", output);
    }

    [Fact]
    public void BackspaceEnd()
    {
        var initialText = "abcdef|";

        var output = PerformKeyInput(initialText, Key.Backspace);

        Assert.Equal("abcde|", output);
    }

    [Fact]
    public void BackspaceMiddle()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.Backspace);

        Assert.Equal("ab|def", output);
    }

    [Fact]
    public void ControlBackspaceEnd()
    {
        var initialText = "abcdef|";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.Backspace);

        Assert.Equal("|", output);
    }

    [Fact]
    public void ControlBackspaceMultipleWords()
    {
        var initialText = "abc def|";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.Backspace);

        Assert.Equal("abc |", output);
    }

    [Fact]
    public void DeleteEnd()
    {
        var initialText = "abcdef|";

        var output = PerformKeyInput(initialText, Key.Delete);

        Assert.Equal("abcdef|", output);
    }

    [Fact]
    public void DeleteMiddle()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.Delete);

        Assert.Equal("abc|ef", output);
    }

    [Fact]
    public void Left()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.Left);

        Assert.Equal("ab|cdef", output);
    }

    [Fact]
    public void ControlLeft()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.Left);

        Assert.Equal("|abcdef", output);
    }

    [Fact]
    public void Right()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.Right);

        Assert.Equal("abcd|ef", output);
    }

    [Fact]
    public void ControlRight()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.Right);

        Assert.Equal("abcdef|", output);
    }

    [Fact]
    public void ShiftLeft()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ShiftLeft, Key.Left);

        Assert.Equal("ab|c>def", output);
    }

    [Fact]
    public void ControlShiftLeft()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.ShiftLeft, Key.Left);

        Assert.Equal("|abc>def", output);
    }

    [Fact]
    public void ShiftRight()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ShiftLeft, Key.Right);

        Assert.Equal("abc<d|ef", output);
    }

    [Fact]
    public void ControlShiftRight()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.ShiftLeft, Key.Right);

        Assert.Equal("abc<def|", output);
    }

    private string PerformTextInput(string initialText, string inputText)
    {
        var input = new Input();
        input.TextInput = inputText;

        return ApplyInput(initialText, input);
    }

    private string PerformKeyInput(string initialText, params ReadOnlySpan<Key> keys)
    {
        var input = new Input();
        foreach (var key in keys)
        {
            input.KeyPressed.Add(key);
            input.KeyDown.Add(key);
        }

        return ApplyInput(initialText, input);
    }

    private string ApplyInput(string initialText, Input input)
    {
        Assert.True(initialText.Count(x => x is '<' or '>') <= 1);
        Assert.True(initialText.Count(x => x == '|') == 1);

        var cursorPosition = initialText.IndexOf('|');
        initialText = initialText.Replace("|", string.Empty);

        var selectionStart = initialText.AsSpan().IndexOfAny(['<', '>']);
        if (selectionStart == -1)
            selectionStart = cursorPosition;
        else
            initialText = initialText.Replace("<", string.Empty).Replace(">", string.Empty);;

        var virtualBuffer = RenderContext.manager.CreateBuffer("TestArena", (UIntPtr)1_000);
        var arena = Ui.Arena = new Arena(virtualBuffer);

        var layoutInfo = FontShaping.LayoutText(CreateTestFont(), initialText, float.MaxValue, TextAlign.Start, false, arena);

        arena.Dispose();

        var resultingString = TextBoxInputHandler.ProcessInput(layoutInfo, input, ref cursorPosition, ref selectionStart);

        resultingString = resultingString.Insert(cursorPosition, "|");

        if (selectionStart < cursorPosition)
        {
            resultingString = resultingString.Insert(selectionStart, "<");
        }
        else if(selectionStart > cursorPosition)
        {
            resultingString = resultingString.Insert(selectionStart + 1, ">");
        }

        return resultingString;
    }

    private ScaledFont CreateTestFont()
    {
        var font = FontLoader.LoadFont("JetBrainsMono-Regular.ttf");
        return new ScaledFont(font, 20);
    }
}