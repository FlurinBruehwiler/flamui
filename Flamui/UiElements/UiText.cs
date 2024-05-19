using Flamui.Layouting;
using SkiaSharp;

namespace Flamui.UiElements;

public class UiText : UiElement
{
    public string Content { get; set; }

    public float PSize { get; set; }

    public ColorDefinition PColor { get; set; }

    public TextAlign PhAlign { get; set; }

    public TextAlign PvAlign { get; set; }

    private static readonly SKPaint Paint = new()
    {
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Thin, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright)
    };

    private static readonly Dictionary<TextPathCacheItem, SKRect> TextPathCache = new();

    private SKRect GetRect(Point offset)
    {
        var key = new TextPathCacheItem(Content, offset.X, offset.Y);
        if (TextPathCache.TryGetValue(key, out var rect))
        {
            return rect;
        }
        var path = Paint.GetTextPath(Content, offset.X, offset.Y);
        path.GetBounds(out rect);
        path.Dispose();
        TextPathCache.Add(key, rect);
        return rect;
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        if (Content == string.Empty)
            return;

        //don't define paint twice!!!! grrr
        Paint.TextSize = PSize;
        Paint.Color = new SKColor(PColor.Red, PColor.Green, PColor.Blue, PColor.Alpha);

        var rect = GetRect(offset);

        Paint.GetFontMetrics(out var metrics);

        var actualX = offset.X;
        var actualY = offset.Y;

        actualY += PvAlign switch
        {
            TextAlign.Start => PSize,
            TextAlign.End => BoxSize.Height,
            TextAlign.Center => BoxSize.Height / 2 - (metrics.Ascent + metrics.Descent) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        actualX += PhAlign switch
        {
            TextAlign.End => BoxSize.Width - rect.Width,
            TextAlign.Center => BoxSize.Width / 2 - rect.Width / 2,
            TextAlign.Start => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

        renderContext.Add(new Text
        {
            Content = Content,
            X = actualX,
            Y = actualY,
            RenderPaint = new TextPaint
            {
                SkColor = new SKColor(PColor.Red, PColor.Green, PColor.Blue, PColor.Alpha),
                TextSize = PSize
            }
        });
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {

        //todo
        // var rect = GetRect();
        //
        // if (PHeight.Kind == SizeKind.Shrink)
        // {
        //     ComputedBounds.H = rect.Height;
        // }
        //
        // if (PWidth.Kind == SizeKind.Shrink)
        // {
        //     ComputedBounds.W = rect.Width;
        // }

        return new BoxSize();
    }

    public UiText Width(float width)
    {
        // PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }
    //
    // public UiText Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    // {
    //     // PHeight = new SizeDefinition(height, sizeKind);
    //     return this;
    // }

    public UiText Color(byte red, byte green, byte blue, byte transparency = 255)
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

public record struct TextPathCacheItem(string Content, float X, float Y);

public enum TextAlign
{
    Start,
    Center,
    End
}
