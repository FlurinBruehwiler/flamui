using Flamui.UiElements;

namespace Flamui.Drawing;

public enum TextTrimMode
{
    AddEllipsis = 0,
    None,
    TrimToFit,
}

//todo, we should not do font shaping ourselves, it is way to complicated, we should use https://github.com/JimmyLefevre/kb/blob/main/kb_text_shape.h
public sealed class FontShaping
{
    public static (float start, float end) GetPositionOfChar(ScaledFont scaledFont, ReadOnlySpan<char> singleLine, int index)
    {
        float horizontalPosition = 0;

        if (index < 0 || index > singleLine.Length)
            return default;

        for (var i = 0; i < index + 1; i++)
        {
            var c = singleLine[i];

            var start = horizontalPosition;

            horizontalPosition += scaledFont.GetAdvanceWith(c);

            if (index == i)
            {
                return (start, horizontalPosition);
            }
        }

        return default;
    }

    /// <returns>The width of the text</returns>
    public static (float totalWidth, Slice<float> charOffsets) MeasureText(ScaledFont scaledFont, ArenaString singleLine, Arena arena)
    {
        var charOffsets = arena.AllocateSlice<float>(singleLine.Length);

        var width = 0f;
        for (var i = 0; i < singleLine.Length; i++)
        {
            var c = singleLine[i];

            width += scaledFont.GetAdvanceWith(c);
            charOffsets[i] = width;
        }

        return (width, charOffsets);
    }

    //not sure if it is a good idea to have a separate function for single line...
    public static unsafe TextLayoutInfo LayoutSingleLineText(ScaledFont scaledFont, ArenaString text, float maxWidth, Arena arena, TextAlign horizontalAlignement, TextTrimMode trimMode)
    {
        float ellipsisAdvanceWidth = scaledFont.GetAdvanceWith('.') * 3;

        TrimText(trimMode);

        var r = new Range(new Index(0), new Index(text.Length));
        var (width, charOffsets) = MeasureText(scaledFont, text[r], arena);

        Line* line = arena.Allocate(new Line
        {
            TextContent = text[r],
            Bounds = new Bounds
            {
                W = width,
                H = scaledFont.GetHeight(),
                X = 0, //will be set later
                Y = 0
            },
            CharOffsets = charOffsets
        });

        var yCoord = 0f;

        line->Bounds = line->Bounds with
        {
            X = horizontalAlignement switch
            {
                TextAlign.Start => 0,
                TextAlign.Center => (maxWidth - line->Bounds.W) / 2,
                TextAlign.End => maxWidth - line->Bounds.W,
                _ => throw new ArgumentOutOfRangeException()
            },
            Y = yCoord,
        };

        return new TextLayoutInfo
        {
            Lines = new Slice<Line>(line, 1),
            Width = width,
            Height = scaledFont.GetHeight(),
            Content = text
        };

        void TrimText(TextTrimMode innerTrimMode)
        {
            if (innerTrimMode == TextTrimMode.None)
                return;

            var totalWidth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                var charWidth = scaledFont.GetAdvanceWith(c);
                totalWidth += charWidth;

                if (totalWidth <= maxWidth)
                    continue;

                if (innerTrimMode == TextTrimMode.AddEllipsis)
                {
                    //walk back to figure out where to insert the ellipsis
                    for (int j = i; j > 0; j--)
                    {
                        var charWidth1 = scaledFont.GetAdvanceWith(text[j]);

                        totalWidth -= charWidth1;

                        if (totalWidth + ellipsisAdvanceWidth < maxWidth)
                        {
                            text = text.Slice(0, j) + "...".AsArenaStringLiteral();

                            return;
                        }
                    }

                    //not even the ellipsis is fitting, try without
                    TrimText(TextTrimMode.TrimToFit);
                    return;
                }
                else
                {
                    text = text.Slice(0, i);
                }
            }
        }
    }

    //rule: preferably only ever the start of a new word can go onto the next line,
    //so we make a new line, as soon as the next word + following whitespace doesn't fit on the current line
    //if we can't even fit a single word on a line, we have to start to split in the middle of the word!
    public static TextLayoutInfo LayoutText(ScaledFont scaledFont, ArenaString text, float maxWidth, TextAlign horizontalAlignement, bool multilineAllowed, Arena arena)
    {
        ArenaList<Line> lines = new ArenaList<Line>(arena, 1);
        float widthOfLongestLine = 0;

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

            if (c is '\n' or '\r' && multilineAllowed)
            {
                if (c == '\r' && text.Length > i + 1 && text[i + 1] == '\n')
                    i++;

                //add new line
                AddLine(i, text);
                currentLineWidth = 0;
                currentLineStart = i + 1;
                currentBlockStart = i + 1;
                continue;
            }

            var charWidth = scaledFont.GetAdvanceWith(c);

            currentLineWidth += charWidth;
            currentBlockWidth += charWidth;

            if (currentLineWidth > maxWidth && multilineAllowed)
            {
                if (currentLineStart == currentBlockStart) //not even a single word fits onto the line
                {
                    AddLine(i, text);
                    currentLineStart = i;
                    currentBlockStart = i;
                    currentLineWidth = charWidth;
                    currentBlockWidth = charWidth;
                }
                else
                {
                    //add new line
                    AddLine(currentBlockStart, text);
                    currentLineWidth = currentBlockWidth;
                    currentLineStart = currentBlockStart;
                }
            }
        }

        AddLine(text.Length, text);

        var yCoord = 0f;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            var bounds = line.Bounds with
            {
                X = horizontalAlignement switch
                {
                    TextAlign.Start => 0,
                    TextAlign.Center => (maxWidth - line.Bounds.W) / 2,
                    TextAlign.End => maxWidth - line.Bounds.W,
                    _ => throw new ArgumentOutOfRangeException()
                },
                Y = yCoord,
            };

            lines[i] = line with
            {
                Bounds = bounds
            };

            yCoord += scaledFont.GetHeight() + scaledFont.LineGap;
        }

        if(lines.Count == 0)
            AddLine(0, new ArenaString());

        return new TextLayoutInfo
        {
            Lines = lines.AsSlice(),
            Width = widthOfLongestLine,
            Height = lines.Count * scaledFont.GetHeight() + lines.Count - 1 * (scaledFont.LineGap),
            Content = text
        };

        void AddLine(int endIndex, ArenaString t)
        {
            var r = new Range(new Index(currentLineStart), new Index(endIndex));
            var (width, charOffsets) = MeasureText(scaledFont, t[r], arena);
            widthOfLongestLine = Math.Max(widthOfLongestLine, width);
            lines.Add(new Line
            {
                TextContent = text[r],
                Bounds = new Bounds
                {
                    W = width,
                    H = scaledFont.GetHeight(),
                    X = 0, //will be set later
                    Y = 0
                },
                CharOffsets = charOffsets
            });
        }
    }
}

public struct TextLayoutInfo
{
    public required ArenaString Content;
    public Slice<Line> Lines;
    public float Width;
    public float Height;
}

public struct Line
{
    public Bounds Bounds;
    public ArenaString TextContent;
    public Slice<float> CharOffsets; // the start of each char on the x axis
}