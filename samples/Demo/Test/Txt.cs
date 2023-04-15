using System.ComponentModel;
using SkiaSharp;

namespace Demo.Test;

public class Txt : RenderObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string PText { get; set; } = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PSize { get; set; } = 15;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextAlign PhAlign { get; set; } = TextAlign.Start;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextAlign PvAlign { get; set; } = TextAlign.Start;

    public Txt Content(string txt)
    {
        PText = txt;
        return this;
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

    public override void Render()
    {
        var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255),
            IsAntialias = true,
            TextSize = PSize,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var path = paint.GetTextPath(PText, PComputedX, PComputedY);
        path.GetBounds(out var rect);

        var actualX = PComputedX;
        var actualY = PComputedY;

        if (PvAlign == TextAlign.Start)
        {
            actualY += rect.Height;
        }
        else if (PvAlign == TextAlign.End)
        {
            actualY += PComputedHeight;
        }
        else if (PvAlign == TextAlign.Center)
        {
            actualY += PComputedHeight / 2 + rect.Height / 2;
        }

        if (PhAlign == TextAlign.End)
        {
            actualX += PComputedWidth - rect.Width;
        }
        else if (PhAlign == TextAlign.Center)
        {
            actualX += PComputedWidth / 2 - rect.Width / 2;
        }

        Program.Canvas.DrawText(PText, actualX, actualY, paint);
    }
}

public enum TextAlign
{
    Start,
    Center,
    End
}