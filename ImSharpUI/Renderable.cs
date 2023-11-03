using SkiaSharp;

namespace ImSharpUISample;

public class RenderContext
{
    public Dictionary<int, RenderSection> RenderSections = new();
    public Stack<int> ZIndexes = new();

    public RenderContext()
    {
        ZIndexes.Push(0);
    }

    public void Add(IRenderable renderable)
    {
        if (!RenderSections.TryGetValue(ZIndexes.Peek(), out var renderSection))
        {
            renderSection = new RenderSection();
            RenderSections.Add(ZIndexes.Peek(), renderSection);
        }

        renderSection.Renderables.Add(renderable);
    }

    public void Render(SKCanvas canvas)
    {
        foreach (var (_, value) in RenderSections.OrderBy(x => x.Key))
        {
            value.Render(canvas);
        }
    }

    public void SetIndex(int idx)
    {
        ZIndexes.Push(idx);
    }

    public void Restore()
    {
        ZIndexes.Pop();
    }
}

public class RenderSection
{
    public int ZIndex;
    public List<IRenderable> Renderables = new();

    public void Render(SKCanvas canvas)
    {
        foreach (var renderable in Renderables)
        {
            renderable.Render(canvas);
        }
    }
}

public interface IRenderable
{
    public void Render(SKCanvas canvas);
}

public struct Save : IRenderable
{
    public void Render(SKCanvas canvas)
    {
        canvas.Save();
    }
}

public struct Restore : IRenderable
{
    public void Render(SKCanvas canvas)
    {
        canvas.Restore();
    }
}

public struct RectClip : IRenderable
{
    private static readonly SKRoundRect RoundRect = new();

    public required float X;
    public required float Y;
    public required float W;
    public required float H;
    public required float Radius;
    public required SKClipOperation ClipOperation;

    public void Render(SKCanvas canvas)
    {
        if (Radius == 0)
        {
            canvas.ClipRect(SKRect.Create(X, Y, W, H), ClipOperation, true);
        }
        else
        {
            RoundRect.SetRect(SKRect.Create(X, Y, W, H), Radius, Radius);
            canvas.ClipRoundRect(RoundRect, ClipOperation, antialias: true);
        }
    }
}

public struct Rect : IRenderable
{
    public required float X;
    public required float Y;
    public required float W;
    public required float H;
    public required float Radius;
    public required IRenderPaint RenderPaint;

    public void Render(SKCanvas canvas)
    {
        if (Radius == 0)
        {
            canvas.DrawRect(X, Y, W, H, RenderPaint.GetPaint());
        }
        else
        {
            canvas.DrawRoundRect(X, Y, W, H, Radius, Radius, RenderPaint.GetPaint());
        }
    }
}

public struct Bitmap : IRenderable
{
    public required SKRect Rect;
    public required SKBitmap SkBitmap;
    private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();

    public void Render(SKCanvas canvas)
    {
        canvas.DrawBitmap(SkBitmap, Rect, Paint);
    }
}

public struct Picture : IRenderable
{
    public required SKPicture SkPicture;
    public required SKMatrix SkMatrix;

    public void Render(SKCanvas canvas)
    {
        canvas.DrawPicture(SkPicture, ref SkMatrix);
    }
}

public struct Text : IRenderable
{
    public required string Content;
    public required float X;
    public required float Y;
    public required IRenderPaint RenderPaint;

    public void Render(SKCanvas canvas)
    {
        canvas.DrawText(Content, X, Y, RenderPaint.GetPaint());
    }
}

public struct Matrix : IRenderable
{
    public required SKMatrix SkMatrix;

    public void Render(SKCanvas canvas)
    {
        canvas.SetMatrix(SkMatrix);
    }
}

public struct ResetMatrix : IRenderable
{
    public void Render(SKCanvas canvas)
    {
        canvas.ResetMatrix();
    }
}

public struct Circle : IRenderable
{
    public required SKPaint SkPaint;
    public required SKPoint Pos;
    public required float Radius;

    public void Render(SKCanvas canvas)
    {
        canvas.DrawCircle(Pos, Radius, SkPaint);
    }
}

public struct Path : IRenderable
{
    public required SKPoint Start;
    public required SKPoint StartHandle;
    public required SKPoint EndHandle;
    public required SKPoint End;
    public required SKPaint SkPaint;

    private static SKPath _path = new();

    public void Render(SKCanvas canvas)
    {
        _path.MoveTo(Start);
        _path.CubicTo(StartHandle, EndHandle, End);

        canvas.DrawPath(_path, SkPaint);

        _path.Reset();
    }
}

public interface IRenderPaint
{
    SKPaint GetPaint();
}

public struct TextPaint : IRenderPaint
{
    public required float TextSize;
    public required SKColor SkColor;

    private static readonly SKPaint Paint = MakeTextPaint();

    public SKPaint GetPaint()
    {
        Paint.TextSize = TextSize;
        Paint.Color = SkColor;
        return Paint;
    }

    private static SKPaint MakeTextPaint()
    {
        var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Thin, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);
        return paint;
    }
}

public struct ShadowPaint : IRenderPaint
{
    public required SKColor SkColor;
    public required float ShadowSigma;

    private static readonly Dictionary<float, SKMaskFilter> MaskFilterCache = new();
    private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();

    public SKPaint GetPaint()
    {
        if (!MaskFilterCache.TryGetValue(ShadowSigma, out var maskFilter))
        {
            //todo maybe ensure that not no many unused maskfilters get created??? because maskfilters are disposable :) AND immutable grrrr
            maskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, ShadowSigma, false);
            MaskFilterCache.Add(ShadowSigma, maskFilter);
        }

        Paint.Color = SkColor;
        Paint.MaskFilter = maskFilter;
        return Paint;
    }
}

public struct PlaintPaint : IRenderPaint
{
    public required SKColor SkColor;
    private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();

    public SKPaint GetPaint()
    {
        Paint.Color = SkColor;
        return Paint;
    }
}

public static class Helpers
{
    public static SKPaint GetNewAntialiasedPaint()
    {
        var paint = new SKPaint();
        paint.IsAntialias = true;
        return paint;
    }
}
