using SkiaSharp;

namespace ImSharpUISample.UiElements;

public class UiText : UiElement
{
    public string Content { get; set; }

    public float PSize { get; set; } = 15;

    public ColorDefinition PColor { get; set; } = new(255, 255, 255);

    public TextAlign PhAlign { get; set; } = TextAlign.Start;

    public TextAlign PvAlign { get; set; } = TextAlign.Start;

    public override void Render(SKCanvas canvas)
    {
        if (Content == string.Empty)
            return;

        var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255),
            IsAntialias = true,
            TextSize = PSize,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var path = paint.GetTextPath(Content, PComputedX, PComputedY);
        path.GetBounds(out var rect);

        paint.GetFontMetrics(out var metrics);

        var actualX = PComputedX;
        var actualY = PComputedY;

        actualY += PvAlign switch
        {
            TextAlign.Start => PSize,
            TextAlign.End => PComputedHeight,
            TextAlign.Center => PComputedHeight / 2 - (metrics.Ascent + metrics.Descent) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        actualX += PhAlign switch
        {
            TextAlign.End => PComputedWidth - rect.Width,
            TextAlign.Center => PComputedWidth / 2 - rect.Width / 2,
            TextAlign.Start => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

        canvas.DrawText(Content, actualX, actualY, paint);
    }

    public override void Layout()
    {
    }

    public UiText Width(float width, SizeKind sizeKind = SizeKind.Pixel)
    {
        PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }

    public UiText Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    {
        PHeight = new SizeDefinition(height, sizeKind);
        return this;
    }

    public UiText Color(float red, float green, float blue, float transparency = 255)
    {
        PColor = new ColorDefinition(red, green, blue, transparency);
        return this;
    }

    public UiText Color(ColorDefinition color)
    {
        PColor = color;
        return this;
    }

    public UiText Size(float size)
    {
        PSize = size;
        return this;
    }

    public UiText HAlign(TextAlign textAlign)
    {
        PhAlign = textAlign;
        return this;
    }

    public UiText VAlign(TextAlign textAlign)
    {
        PvAlign = textAlign;
        return this;
    }
}

public enum TextAlign
{
    Start,
    Center,
    End
}
