using System.ComponentModel;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace TolggeUI;

public class Txt : RenderObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string PText { get; set; } = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PSize { get; set; } = 15;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ColorDefinition PColor { get; set; } = new(255, 255, 255);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextAlign PhAlign { get; set; } = TextAlign.Start;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextAlign PvAlign { get; set; } = TextAlign.Start;

    public Txt(string content, [CallerLineNumber] int lineNumer = -1, [CallerFilePath] string filePath = "")
    {
        PText = content;
        PLineNumber = lineNumer;
        PFilePath = filePath;
    }

    public Txt Width(float width, SizeKind sizeKind = SizeKind.Pixel)
    {
        PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }

    public Txt Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    {
        PHeight = new SizeDefinition(height, sizeKind);
        return this;
    }

    public Txt Color(float red, float green, float blue, float transparency = 255)
    {
        PColor = new ColorDefinition(red, green, blue, transparency);
        return this;
    }

    public Txt Color(ColorDefinition color)
    {
        PColor = color;
        return this;
    }

    public Txt Size(float size)
    {
        PSize = size;
        return this;
    }

    public Txt HAlign(TextAlign textAlign)
    {
        PhAlign = textAlign;
        return this;
    }

    public Txt VAlign(TextAlign textAlign)
    {
        PvAlign = textAlign;
        return this;
    }

    static readonly SKPaint Paint = new()
    {
        Color = new SKColor(255, 255, 255),
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright)
    };

    public override void Render(SKCanvas canvas, RenderContext renderContext)
    {
        Paint.TextSize = renderContext.Scale(PSize);

        var path = Paint.GetTextPath(PText, PComputedX, PComputedY);
        path.GetBounds(out var rect);

        Paint.GetFontMetrics(out var metrics);

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

        canvas.DrawText(PText, actualX, actualY, Paint);
    }
}

public enum TextAlign
{
    Start,
    Center,
    End
}
