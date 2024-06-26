using Flamui.Layouting;
using SkiaSharp;

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

    private static readonly SKPaint Paint = new()
    {
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Thin, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright)
    };

    private SKRect GetRect()
    {
        var bounds = new SKRect();
        Paint.MeasureText(UiTextInfo.Content, ref bounds);
        return bounds;
    }

    private void UpdatePaint()
    {
        Paint.TextSize = UiTextInfo.Size;
        Paint.Color = new SKColor(UiTextInfo.Color.Red, UiTextInfo.Color.Green, UiTextInfo.Color.Blue, UiTextInfo.Color.Alpha);
    }

    public override void Reset()
    {
        UiTextInfo = new();
        base.Reset();
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        if (UiTextInfo.Content == string.Empty)
            return;

        UpdatePaint();

        var rect = GetRect();

        Paint.GetFontMetrics(out var metrics);

        var actualX = offset.X;
        var actualY = offset.Y;

        actualY += UiTextInfo.VerticalAlignment switch
        {
            TextAlign.Start => UiTextInfo.Size,
            TextAlign.End => Rect.Height,
            TextAlign.Center => Rect.Height / 2 - (metrics.Ascent + metrics.Descent) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        actualX += UiTextInfo.HorizontalAlignment switch
        {
            TextAlign.End => Rect.Width - rect.Width,
            TextAlign.Center => Rect.Width / 2 - rect.Width / 2,
            TextAlign.Start => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

        renderContext.Add(new Text
        {
            Content = UiTextInfo.Content,
            X = actualX,
            Y = actualY,
            RenderPaint = new TextPaint //don't define paint twice
            {
                SkColor = new SKColor(UiTextInfo.Color.Red, UiTextInfo.Color.Green, UiTextInfo.Color.Blue, UiTextInfo.Color.Alpha),
                TextSize = UiTextInfo.Size
            }
        });
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        UpdatePaint();

        var rect = GetRect();

        Rect = new BoxSize(rect.Width, rect.Height);

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

public record struct TextPathCacheItem(string Content, float X, float Y);

public enum TextAlign
{
    Start,
    Center,
    End
}
