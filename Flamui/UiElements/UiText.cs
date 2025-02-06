﻿using System.Numerics;
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
    public string Content = string.Empty;
    public bool Selectable;
}

public class UiText : UiElement
{
    public UiTextInfo UiTextInfo;
    public TextLayoutInfo TextLayoutInfo;
    public (int line, int character)? DragStart;

    public override void Reset()
    {
        UiTextInfo = new();
        TextLayoutInfo = default;
        base.Reset();
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        if (UiTextInfo.Content == string.Empty)
            return;

        var font = UiTextInfo.Font;

        var scaledFont = new ScaledFont(font, UiTextInfo.Size);

        if (UiTextInfo.Selectable)
        {
            //todo not correct when matrix stuff....
            if (new Bounds(new Vector2(offset.X, offset.Y), new Vector2(Rect.Width, Rect.Height)).ContainsPoint(Window.MousePosition))
            {
                for (var lineIndex = 0; lineIndex < TextLayoutInfo.Lines.Length; lineIndex++)
                {
                    var line = TextLayoutInfo.Lines[lineIndex];
                    var bounds = line.Bounds;
                    bounds.X += offset.X;
                    bounds.Y += offset.Y;

                    if (bounds.ContainsPoint(Window.MousePosition))
                    {
                        var characterIndex = FontShaping.HitTest(scaledFont, line.TextContent.AsSpan(),
                            Window.MousePosition.X - bounds.X);
                        if (characterIndex != -1)
                        {
                            if (DragStart == null)
                                DragStart = (lineIndex, characterIndex);

                            var range = FontShaping.GetPositionOfChar(scaledFont, line.TextContent.AsSpan(), characterIndex);

                            renderContext.AddRect(new Bounds
                            {
                                X = bounds.X + range.start,
                                Y = bounds.Y,
                                W = range.end - range.start,
                                H = bounds.H
                            }, this, C.Blue5);
                        }

                        break;
                    }
                }
            }
        }

        foreach (var line in TextLayoutInfo.Lines)
        {
            var bounds = line.Bounds;
            bounds.X += offset.X;
            bounds.Y += offset.Y;

            renderContext.AddText(bounds, line.TextContent, UiTextInfo.Color, scaledFont);
        }
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        TextLayoutInfo = FontShaping.LayoutText(new ScaledFont(UiTextInfo.Font, UiTextInfo.Size), UiTextInfo.Content, constraint.MaxWidth, UiTextInfo.HorizontalAlignment, UiTextInfo.Multiline);

        Rect = new BoxSize(TextLayoutInfo.Width, TextLayoutInfo.Height);
        return Rect;
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

    public UiText Selectable(bool selectable = true)
    {
        UiTextInfo.Selectable = selectable;
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
