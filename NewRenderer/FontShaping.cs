namespace NewRenderer;

public static class FontShaping
{
    public static (float start, float end) GetPositionOfChar(Font font, ReadOnlySpan<char> singleLine, int index)
    {
        float xCoord = 0;

        if (index < 0 || index > singleLine.Length)
            return default;

        for (var i = 0; i < index + 1; i++)
        {
            var c = singleLine[i];

            var start = xCoord;

            xCoord += font.GetCharWidth(c);

            if (index == i)
            {
                return (start, xCoord);
            }
        }

        return default;
    }

    /// <summary>
    /// Performs a horizontal hit test against a piece of text.
    /// </summary>
    /// <param name="font">The font to use</param>
    /// <param name="singleLine">A piece of text, that lives on a single line</param>
    /// <param name="pos">The position relative to the left of the line</param>
    /// <returns>The index of the char that is under, -1 the pos was outside the text <see cref="pos"/></returns>
    public static int HitTest(Font font, ReadOnlySpan<char> singleLine, float pos)
    {
        float xCoord = 0;

        if (pos < 0)
            return -1;

        for (var i = 0; i < singleLine.Length; i++)
        {
            var c = singleLine[i];

            xCoord += font.GetCharWidth(c);

            if (pos < xCoord)
                return i;
        }

        return -1;
    }

    //rule: preferably only ever the start of a new word can go onto the next line,
    //so we make a new line, as soon as the next word + following whitespace doesn't fit on the current line
    //if we can't even fit a single word on a line, we have to start to split in the middle of the word!
    public static Range[] SplitTextIntoLines(Font font, ReadOnlySpan<char> text, float maxWidth)
    {
        List<Range> ranges = [];

        int currentBlockStart = 0;
        float currentBlockWidth = 0;

        int currentLineStart = 0;
        float currentLineWidth = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (i != 0 && char.IsWhiteSpace(text[i - 1]) && !char.IsWhiteSpace(c))
            {
                currentBlockStart = i;
                currentBlockWidth = 0;
            }

            if (c is '\n' or '\r')
            {
                if (c == '\r' && text.Length > i + 1 && text[i + 1] == '\n')
                    i++;

                //add new line
                ranges.Add(new Range(new Index(currentLineStart), new Index(i)));
                currentLineWidth = 0;
                currentLineStart = i + 1;
                currentBlockStart = i + 1;
                continue;
            }

            var charWidth = font.GetCharWidth(c);

            currentLineWidth += charWidth;
            currentBlockWidth += charWidth;


            if (currentLineWidth > maxWidth)
            {
                if (currentLineStart == currentBlockStart) //not even a single word fits onto the line
                {
                    ranges.Add(new Range(new Index(currentLineStart), new Index(i)));
                    currentLineStart = i;
                    currentBlockStart = i;
                    currentLineWidth = charWidth;
                    currentBlockWidth = charWidth;
                }
                else
                {
                    //add new line
                    ranges.Add(new Range(new Index(currentLineStart), new Index(currentBlockStart)));
                    currentLineWidth = currentBlockWidth;
                    currentLineStart = currentBlockStart;
                }
            }
        }

        ranges.Add(new Range(new Index(currentLineStart), new Index(text.Length)));

        return ranges.ToArray();
    }
}