using Flamui.Drawing;
using Flamui.Layouting;

namespace Flamui.UiElements;

public struct UiTextInfo
{
    public UiTextInfo()
    {
    }

    //todo copy this struct, so we don't need to initialize it every time
    public float Size = 15;
    public ColorDefinition Color = C.Black;
    public TextAlign HorizontalAlignment = TextAlign.Start;
    public TextAlign VerticalAlignment = TextAlign.Center;
    public string Content = string.Empty;
}

public class UiText : UiElement
{
    public UiTextInfo UiTextInfo;


    public override void Reset()
    {
        UiTextInfo = new();
        base.Reset();
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        if (UiTextInfo.Content == string.Empty)
            return;

        var font = Renderer.DefaultFont;

        var width = FontShaping.MeasureText(font, UiTextInfo.Content);

        // Paint.GetFontMetrics(out var metrics);

        var actualX = offset.X;
        var actualY = offset.Y;

        actualY += UiTextInfo.VerticalAlignment switch
        {
            TextAlign.Start => UiTextInfo.Size,
            TextAlign.End => Rect.Height,
            TextAlign.Center => Rect.Height / 2 - font.GetHeight() / 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        actualX += UiTextInfo.HorizontalAlignment switch
        {
            TextAlign.End => Rect.Width - width,
            TextAlign.Center => Rect.Width / 2 - width / 2,
            TextAlign.Start => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

        var bounds = new Bounds
        {
            X = actualX,
            Y = actualY,
            W = width,
            H = font.GetHeight()
        };
        renderContext.AddText(bounds, UiTextInfo.Content, UiTextInfo.Color, Renderer.DefaultFont);
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        Rect = new BoxSize(FontShaping.MeasureText(Renderer.DefaultFont, UiTextInfo.Content), Renderer.DefaultFont.GetHeight());

        return Rect;
    }

    public UiText Width(float width)
    {
        // PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }

    public UiText Color(byte red, byte green, byte blue, byte transparency = 255)
    {
        UiTextInfo.Color = new ColorDefinition(red, green, blue, transparency);
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

    public UiText HAlign(TextAlign textAlign)
    {
        UiTextInfo.HorizontalAlignment = textAlign;
        return this;
    }

    public UiText VAlign(TextAlign textAlign)
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
