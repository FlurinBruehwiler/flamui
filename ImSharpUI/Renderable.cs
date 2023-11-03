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

    public bool Render(SKCanvas canvas, RenderContext lastRenderContext)
    {
        foreach (var (key, value) in RenderSections)
        {
            if (!lastRenderContext.RenderSections.TryGetValue(key, out var lastRenderSection))
            {
                Rerender(canvas);
                return true;
            }

            if (lastRenderSection.Renderables.Count != value.Renderables.Count)
            {
                Rerender(canvas);
                return true;
            }

            for (var i = 0; i < value.Renderables.Count; i++)
            {
                var renderable = value.Renderables[i];
                var lastRerenderable = lastRenderSection.Renderables[i];

                if (!renderable.UiEquals(lastRerenderable))
                {
                    Rerender(canvas);
                    return true;
                }
            }
        }

        return false;
    }

    private static int rerenderCount;

    private void Rerender(SKCanvas canvas)
    {
        // Console.WriteLine($"Rerender: {++rerenderCount}");
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
    public bool UiEquals(IRenderable renderable);
}

public struct Save : IRenderable
{
    public void Render(SKCanvas canvas)
    {
        canvas.Save();
    }

    public bool UiEquals(IRenderable renderable)
    {
        return renderable is Save;
    }
}

public struct Restore : IRenderable
{
    public void Render(SKCanvas canvas)
    {
        canvas.Restore();
    }

    public bool UiEquals(IRenderable renderable)
    {
        return renderable is Restore;
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

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not RectClip rectClip)
            return false;

        return rectClip.X == X
               && rectClip.Y == Y
               && rectClip.W == W
               && rectClip.H == H
               && rectClip.Radius == Radius
               && rectClip.ClipOperation == ClipOperation;
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

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not Rect rect)
            return false;

        return rect.X == X
               && rect.Y == Y
               && rect.W == W
               && rect.H == H
               && rect.Radius == Radius
               && rect.RenderPaint.PaintEquals(RenderPaint);
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

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not Bitmap bitmap)
            return false;

        return bitmap.Rect == Rect;
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

    public bool UiEquals(IRenderable renderable)
    {
        return renderable is Picture;
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

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not Text text)
            return false;

        return text.Content == Content && text.X == X && text.Y == Y && text.RenderPaint.PaintEquals(RenderPaint);
    }
}

public struct Matrix : IRenderable
{
    public required SKMatrix SkMatrix;

    public void Render(SKCanvas canvas)
    {
        canvas.SetMatrix(SkMatrix);
    }

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not Matrix matrix)
            return false;

        return matrix.SkMatrix == SkMatrix;
    }
}

public struct ResetMatrix : IRenderable
{
    public void Render(SKCanvas canvas)
    {
        canvas.ResetMatrix();
    }

    public bool UiEquals(IRenderable renderable)
    {
        return renderable is ResetMatrix;
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

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not Circle circle)
            return false;

        return circle.Pos == Pos && circle.Radius == Radius;
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

    public bool UiEquals(IRenderable renderable)
    {
        if (renderable is not Path path)
            return false;

        return path.Start == Start && path.StartHandle == StartHandle && path.EndHandle == EndHandle &&
               path.End == End;
    }
}

public interface IRenderPaint
{
    SKPaint GetPaint();
    bool PaintEquals(IRenderPaint renderPaint);
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

    public bool PaintEquals(IRenderPaint renderPaint)
    {
        if (renderPaint is not TextPaint textPaint)
            return false;

        return textPaint.TextSize == TextSize && textPaint.SkColor == SkColor;
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

    public bool PaintEquals(IRenderPaint renderPaint)
    {
        if (renderPaint is not ShadowPaint shadowPaint)
            return false;

        return shadowPaint.SkColor == SkColor && shadowPaint.ShadowSigma == ShadowSigma;
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

    public bool PaintEquals(IRenderPaint renderPaint)
    {
        if (renderPaint is not PlaintPaint plaintPaint)
            return false;

        return plaintPaint.SkColor == SkColor;
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
