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
        Offset = offset;

        if (UiTextInfo.Content == string.Empty)
            return;

        var font = UiTextInfo.Font;

        // var width = ;
        //
        // // Paint.GetFontMetrics(out var metrics);
        //
        // var actualX = offset.X;
        // var actualY = offset.Y;
        //
        // actualY += UiTextInfo.VerticalAlignment switch
        // {
        //     TextAlign.Start => UiTextInfo.Size,
        //     TextAlign.End => Rect.Height,
        //     TextAlign.Center => Rect.Height / 2 - font.GetHeight() / 2,
        //     _ => throw new ArgumentOutOfRangeException()
        // };
        //
        // actualX += UiTextInfo.HorizontalAlignment switch
        // {
        //     TextAlign.End => Rect.Width - width,
        //     TextAlign.Center => Rect.Width / 2 - width / 2,
        //     TextAlign.Start => 0,
        //     _ => throw new ArgumentOutOfRangeException()
        // };

        var scale = font.GetScale(UiTextInfo.Size);

        var entireText = UiTextInfo.Content.AsSpan();

        var yCoord = offset.Y;
        foreach (var line in TextLayoutInfo.Lines)
        {
            var lineSpan = entireText[line.TextContent];

            var bounds = new Bounds
            {
                X = UiTextInfo.HorizontalAlignment switch
                {
                    TextAlign.Start => offset.X,
                    TextAlign.Center => offset.X + (MaxWidth - line.Width) / 2,
                    TextAlign.End => offset.X + (MaxWidth - line.Width),
                    _ => throw new ArgumentOutOfRangeException()
                },
                Y = yCoord,
                W = line.Width,
                H = font.GetHeight(scale)
            };

            //debug rect
            // renderContext.AddRect(bounds, this, new ColorDefinition(100, 0, 0, 100));

            //avoid to string and use arenastring!!!!!!!
            renderContext.AddText(bounds, lineSpan.ToString(), UiTextInfo.Color, font, UiTextInfo.Size);

            yCoord += font.GetHeight(scale) + font.UnscaledLineGap;
        }
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        var scale = UiTextInfo.Font.GetScale(UiTextInfo.Size);

        TextLayoutInfo = FontShaping.LayoutText(UiTextInfo.Font, scale, UiTextInfo.Content, constraint.MaxWidth);
        MaxWidth = constraint.MaxWidth;

        Rect = new BoxSize(TextLayoutInfo.MaxWidth, TextLayoutInfo.TotalHeight);
        return Rect;
    }

    public TextLayoutInfo TextLayoutInfo;
    private float MaxWidth;

    public Point Offset;

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
