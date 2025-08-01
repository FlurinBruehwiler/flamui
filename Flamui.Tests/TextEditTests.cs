﻿using Flamui.Components;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Input;

namespace Flamui.Tests;

public sealed class TextEditTests
{
    // |      the cursor
    // </>    the start of the selected text

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
    public void BackspaceSelection()
    {
        var initialText = "abc<def|";

        var output = PerformKeyInput(initialText, Key.Backspace);

        Assert.Equal("abc|", output);
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
    public void DeleteSelection()
    {
        var initialText = "a<bc|def";

        var output = PerformKeyInput(initialText, Key.Delete);

        Assert.Equal("a|def", output);
    }

    [Fact]
    public void ControlDeleteEnd()
    {
        var initialText = "abcdef|";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.Delete);

        Assert.Equal("abcdef|", output);
    }

    [Fact]
    public void ControlDeleteMultipleWords()
    {
        var initialText = "abc| def ghi";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.Delete);

        Assert.Equal("abc| ghi", output);
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

    [Fact]
    public void Paste()
    {
        var initialText = "abc|def";

        var output = PerformKeyInputWithHost(initialText, new TestUiTreeHost(clipboardText:  "ghi"), Key.ControlLeft, Key.V);

        Assert.Equal("abcghi|def", output);
    }

    [Fact]
    public void PasteOverSelection()
    {
        var initialText = "abc|de>f";

        var output = PerformKeyInputWithHost(initialText, new TestUiTreeHost(clipboardText:  "ghi"), Key.ControlLeft, Key.V);

        Assert.Equal("abcghi|f", output);
    }

    [Fact]
    public void Cut()
    {
        var initialText = "abc|de>f";

        var host = new TestUiTreeHost(string.Empty);

        var output = PerformKeyInputWithHost(initialText, host, Key.ControlLeft, Key.X);

        Assert.Equal("abc|f", output);
        Assert.Equal("de", host.GetClipboardText());
    }

    [Fact]
    public void CutWithoutSelection()
    {
        var initialText = "abc|def";

        var host = new TestUiTreeHost("hi");

        var output = PerformKeyInputWithHost(initialText, host, Key.ControlLeft, Key.X);

        Assert.Equal("abc|def", output);
        Assert.Equal("hi", host.GetClipboardText());
    }

    [Fact]
    public void Copy()
    {
        var initialText = "abc|de>f";

        var host = new TestUiTreeHost(string.Empty);

        var output = PerformKeyInputWithHost(initialText, host, Key.ControlLeft, Key.C);

        Assert.Equal("abc|de>f", output);
        Assert.Equal("de", host.GetClipboardText());
    }

    [Fact]
    public void CopyWithoutSelection()
    {
        var initialText = "abc|def";

        var host = new TestUiTreeHost("hi");

        var output = PerformKeyInputWithHost(initialText, host, Key.ControlLeft, Key.C);

        Assert.Equal("abc|def", output);
        Assert.Equal("hi", host.GetClipboardText());
    }

    [Fact]
    public void DeleteAll()
    {
        var initialText = "<abcdef|";

        var output = PerformKeyInput(initialText, Key.Backspace);

        Assert.Equal("|", output);
    }

    [Fact]
    public void SelectAll()
    {
        var initialText = "abc|def";

        var output = PerformKeyInput(initialText, Key.ControlLeft, Key.A);

        Assert.Equal("<abcdef|", output);
    }

    [Fact]
    public void SelectionActiveAndPressingShiftShouldNotDoAnything()
    {
        var initialText = "a<bcde|f";

        var output = PerformKeyInput(initialText, Key.ShiftLeft);

        Assert.Equal("a<bcde|f", output);
    }

    [Fact]
    public void SelectionActiveAndPressingShiftShouldNotDoAnything2()
    {
        var initialText = "a|bcde>f";

        var output = PerformKeyInput(initialText, Key.ShiftLeft);

        Assert.Equal("a|bcde>f", output);
    }

    [Fact]
    public void SelectionThenRight()
    {
        var initialText = "a|bcde>f";

        var output = PerformKeyInput(initialText, Key.Right);

        Assert.Equal("abcde|f", output);
    }

    [Fact]
    public void SelectionThenLeft()
    {
        var initialText = "a|bcde>f";

        var output = PerformKeyInput(initialText, Key.Left);

        Assert.Equal("a|bcdef", output);
    }

    [Fact]
    public void SelectionOtherThenRight()
    {
        var initialText = "a<bcde|f";

        var output = PerformKeyInput(initialText, Key.Right);

        Assert.Equal("abcde|f", output);
    }

    [Fact]
    public void SelectionOtherThenLeft()
    {
        var initialText = "a<bcde|f";

        var output = PerformKeyInput(initialText, Key.Right);

        Assert.Equal("abcde|f", output);
    }

    [Fact]
    public void SelectedRangeTest()
    {
        var text = "a<bcde|f";

        var (cursorPosition, selectionStart) = ExtractMetadata(ref text);

        var range = TextBoxInputHandler.GetSelectedRange(cursorPosition, selectionStart);

        var subString = text[range.ToRange()];

        Assert.Equal("bcde", subString);
    }

    [Fact]
    public void SelectedRangeOtherTest()
    {
        var text = "a|bcde>f";

        var (cursorPosition, selectionStart) = ExtractMetadata(ref text);

        var range = TextBoxInputHandler.GetSelectedRange(cursorPosition, selectionStart);

        var subString = text[range.ToRange()];

        Assert.Equal("bcde", subString);
    }

    [Fact]
    public void SelectedRangeEmptyTest()
    {
        var text = "a|bcdef";

        var (cursorPosition, selectionStart) = ExtractMetadata(ref text);

        var range = TextBoxInputHandler.GetSelectedRange(cursorPosition, selectionStart);

        var subString = text[range.ToRange()];

        Assert.Equal("", subString);
    }

    [Fact]
    public void GetWordUnderCursorTest()
    {
        var (start, end) = UiExtensions.GetWordUnderCursor("anita max wynn", 6);
        Assert.Equal(6, start);
        Assert.Equal(9, end);

        (start, end) = UiExtensions.GetWordUnderCursor("anita max wynn", 7);
        Assert.Equal(6, start);
        Assert.Equal(9, end);

        (start, end) = UiExtensions.GetWordUnderCursor("anita max wynn", 8);
        Assert.Equal(6, start);
        Assert.Equal(9, end);

        (start, end) = UiExtensions.GetWordUnderCursor("anita max wynn", 9);
        Assert.Equal(6, start);
        Assert.Equal(9, end);

        (start, end) = UiExtensions.GetWordUnderCursor("anita max wynn", 4);
        Assert.Equal(0, start);
        Assert.Equal(5, end);

        (start, end) = UiExtensions.GetWordUnderCursor("anita max wynn", 11);
        Assert.Equal(10, start);
        Assert.Equal(14, end);
    }

    private string PerformTextInput(string initialText, string inputText)
    {
        var input = new UiTree();
        input.TextInput = inputText;

        return ApplyInput(initialText, input);
    }

    private string PerformKeyInput(string initialText, params ReadOnlySpan<Key> keys)
    {
        return PerformKeyInputWithHost(initialText, new TestUiTreeHost(string.Empty), keys);
    }

    private string PerformKeyInputWithHost(string initialText, TestUiTreeHost uiTreeHost, params ReadOnlySpan<Key> keys)
    {
        var input = new UiTree();
        input.UiTreeHost = uiTreeHost;

        foreach (var key in keys)
        {
            input.KeyPressed.Add(key);
            input.KeyDown.Add(key);
        }

        return ApplyInput(initialText, input);
    }

    private (int cursorPosition, int selectionStart) ExtractMetadata(ref string initialText)
    {
        Assert.True(initialText.Count(x => x is '<' or '>') <= 1);
        Assert.True(initialText.Count(x => x == '|') == 1);

        var cursorPosition = initialText.IndexOf('|');
        initialText = initialText.Replace("|", string.Empty);

        var selectionStart = initialText.AsSpan().IndexOfAny(['<', '>']);
        if (selectionStart == -1)
            selectionStart = cursorPosition;
        else
        {
            initialText = initialText.Replace("<", string.Empty).Replace(">", string.Empty);
            if (selectionStart < cursorPosition)
                cursorPosition--;
        }

        return (cursorPosition, selectionStart);
    }

    private string ApplyInput(string initialText, UiTree input)
    {
        var (cursorPosition, selectionStart) = ExtractMetadata(ref initialText);

        var arena = Ui.Arena = new Arena("TestArena", 1_000);

        var font = FontLoader.LoadFont("segoeui.ttf");
        var layoutInfo = FontShaping.LayoutSingleLineText(new ScaledFont(font, 20), initialText, float.MaxValue, arena, TextAlign.Start, TextTrimMode.None);

        arena.Dispose();

        var resultingString = TextBoxInputHandler.ProcessInput(initialText, layoutInfo, input, false, ref cursorPosition, ref selectionStart);

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
}