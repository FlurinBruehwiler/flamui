using Flamui.Drawing;
using Flamui.Layouting;

namespace Flamui.UiElements;

public struct UiTextInfo
{
    public UiTextInfo()
    {
    }

    //todo copy this struct, so we don't need to initialize it every time
    public float Size; //default comes from cascading values
    public ColorDefinition Color; //default comes from cascading values
    public Font? Font; //default comes from cascading values

    public TextAlign HorizontalAlignment = TextAlign.Start;
    public TextAlign VerticalAlignment = TextAlign.Center;
    public bool Multiline;
    public ArenaString Content = default;
    public TextTrimMode TrimMode;
}

public struct TextPosition
{
    public TextPosition(int line, int character)
    {
        Line = line;
        Character = character;
    }

    public int Line;
    public int Character;
}

public sealed class UiText : UiElement
{
    public UiTextInfo UiTextInfo;
    public TextLayoutInfo TextLayoutInfo;

    //the cursor + selection shouldn't really be handled by this UiElement i think, but not sure tbh :)
    public int SelectionStart = 0;
    public int CursorPosition = 0;
    public bool ShowCursor = false;

    //public (int line, int character)? DragStart;

    public override void Reset()
    {
        UiTextInfo = new();
        //TextLayoutInfo = default;
        base.Reset();
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        // if (UiTextInfo.Content == string.Empty)
        //     return;

        var font = UiTextInfo.Font;

        var scaledFont = new ScaledFont(font, UiTextInfo.Size);

        // var offset = TextLayoutInfo.Lines[0].CharOffsets[CursorIndex];

        var o = 0;

        for (var i = 0; i < TextLayoutInfo.Lines.Length; i++)
        {
            var line = TextLayoutInfo.Lines[i];

            var bounds = line.Bounds;
            bounds.X += offset.X;
            bounds.Y += offset.Y;

            float selectionCharStartOffset = -1;
            float cursorCharOffset = -1;

            if (o <= SelectionStart && ShowCursor)
            {
                var cursorOffsetOnLine = SelectionStart - o;
                selectionCharStartOffset = cursorOffsetOnLine == 0 ? 0 : line.CharOffsets[cursorOffsetOnLine - 1];
            }

            if (o <= CursorPosition && ShowCursor)
            {
                var cursorOffsetOnLine = CursorPosition - o;
                cursorCharOffset = cursorOffsetOnLine == 0 ? 0 : line.CharOffsets[cursorOffsetOnLine - 1];
                renderContext.AddRect(new Bounds
                {
                    X = bounds.X + cursorCharOffset,
                    Y = bounds.Y,
                    W = 1,
                    H = bounds.H
                }, this, C.White);
            }

            if (selectionCharStartOffset != -1f && cursorCharOffset != -1f &&
                selectionCharStartOffset != cursorCharOffset)
            {
                renderContext.AddRect(new Bounds
                {
                    X = bounds.X + Math.Min(selectionCharStartOffset, cursorCharOffset),
                    Y = bounds.Y,
                    W = Math.Abs(selectionCharStartOffset - cursorCharOffset),
                    H = bounds.H
                }, this, C.Blue6 / 2);
            }

            renderContext.AddText(this, bounds, line.TextContent, UiTextInfo.Color, scaledFont);
        }
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        if (UiTextInfo.Multiline)
        {
            TextLayoutInfo = FontShaping.LayoutText(new ScaledFont(UiTextInfo.Font, UiTextInfo.Size), UiTextInfo.Content,
                constraint.MaxWidth, UiTextInfo.HorizontalAlignment, UiTextInfo.Multiline, Tree.Arena);
        }
        else
        {
            TextLayoutInfo = FontShaping.LayoutSingleLineText(new ScaledFont(UiTextInfo.Font, UiTextInfo.Size), UiTextInfo.Content,
                constraint.MaxWidth, Tree.Arena, UiTextInfo.HorizontalAlignment, UiTextInfo.TrimMode);
        }

        Rect = new BoxSize(TextLayoutInfo.Width, TextLayoutInfo.Height);
        return Rect;
    }

    /// <summary>
    /// The default trim mode is ellipsis
    /// </summary>
    public UiText TrimMode(TextTrimMode trimMode)
    {
        UiTextInfo.TrimMode = trimMode;
        return this;
    }

    public UiText Color(byte red, byte green, byte blue, byte transparency = 255)
    {
        UiTextInfo.Color = new ColorDefinition(red, green, blue, transparency);
        return this;
    }

    public UiText Font(Font font)
    {
        UiTextInfo.Font = font;
        return this;
    }

    public UiText Color(ColorDefinition color)
    {
        UiTextInfo.Color = color;
        return this;
    }

    public UiText Size(float size)
    {
        UiTextInfo.Size = size;
        return this;
    }

    public UiText Multiline(bool multiline = true)
    {
        UiTextInfo.Multiline = multiline;
        return this;
    }

    public UiText HorizontalAlign(TextAlign textAlign)
    {
        UiTextInfo.HorizontalAlignment = textAlign;
        return this;
    }

    public UiText VerticalAlign(TextAlign textAlign)
    {
        UiTextInfo.VerticalAlignment = textAlign;
        return this;
    }
}

public enum TextAlign
{
    Start,
    Center,
    End
}
