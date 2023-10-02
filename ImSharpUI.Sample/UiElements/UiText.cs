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
}

public enum TextAlign
{
    Start,
    Center,
    End
}
