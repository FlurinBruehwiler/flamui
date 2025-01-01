using System.Numerics;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Maths;

namespace Flamui;

public class RenderContext
{
    public Dictionary<int, RenderSection> RenderSections = new();
    public Stack<int> ZIndexes = new();

    public RenderContext()
    {
        ZIndexes.Push(0);
    }

    public void Reset()
    {
        foreach (var (key, value) in RenderSections)
        {
            value.Renderables.Clear();
        }
    }

    public void Add(IRenderFragment renderFragment)
    {
        if (!RenderSections.TryGetValue(ZIndexes.Peek(), out var renderSection))
        {
            renderSection = new RenderSection();
            RenderSections.Add(ZIndexes.Peek(), renderSection);
        }

        renderSection.Renderables.Add(renderFragment);
    }

    public bool RequiresRerender(RenderContext lastRenderContext)
    {
        foreach (var (key, value) in RenderSections)
        {
            if (!lastRenderContext.RenderSections.TryGetValue(key, out var lastRenderSection))
            {
                return true;
            }

            if (lastRenderSection.Renderables.Count != value.Renderables.Count)
            {
                return true;
            }

            for (var i = 0; i < value.Renderables.Count; i++)
            {
                var renderable = value.Renderables[i];
                var lastRerenderable = lastRenderSection.Renderables[i];

                if (!renderable.UiEquals(lastRerenderable))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int rerenderCount;

    public void Rerender(GlCanvas canvas)
    {
        var sections = RenderSections.OrderBy(x => x.Key).ToList();

        foreach (var (_, value) in sections)
        {
            value.Render(canvas);
        }
    }

    public void SetIndex(int idx)
    {
        ZIndexes.Push(idx);
    }

    public void RestoreZIndex()
    {
        ZIndexes.Pop();
    }
}

public class RenderSection
{
    public List<IRenderFragment> Renderables = new(1000);

    public void Render(GlCanvas canvas)
    {
        foreach (var renderable in Renderables)
        {
            renderable.Render(canvas);
        }
    }
}

public interface IClickableFragment
{
    public UiElement UiElement { get; set; }
    public Bounds Bounds { get; set; }
}

public interface IRenderFragment
{
    public void Render(GlCanvas canvas);
    public bool UiEquals(IRenderFragment renderFragment);
}

public interface IMatrixable
{
    Vector2D<float> ProjectPoint(Vector2 point);
}

// public struct Save : IRenderFragment
// {
//     public void Render(SKCanvas canvas)
//     {
//         canvas.Save();
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         return renderFragment is Save;
//     }
// }

// public struct Restore : IRenderFragment
// {
//     public void Render(SKCanvas canvas)
//     {
//         canvas.Restore();
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         return renderFragment is Restore;
//     }
// }

public struct Bounds
{
    public required float X;
    public required float Y;
    public required float W;
    public required float H;

    public float Right => X + W;

    public bool ContainsPoint(Vector2 point)
    {
        var withinX = point.X >= X && point.X <= X + W;
        var withinY = point.Y >= Y && point.Y <= Y + H;

        return withinX && withinY;
    }
    //
    // public SKRect ToRect()
    // {
    //     return SKRect.Create(X, Y, W, H);
    // }

    public bool BoundsEquals(Bounds otherBounds)
    {
        return otherBounds.X == X
            && otherBounds.Y == Y
            && otherBounds.W == W
            && otherBounds.H == H;
    }

    public Bounds Inflate(float x, float y)
    {
        return new Bounds
        {
            X = X - x,
            Y = Y - y,
            W = W + 2 * x,
            H = H + 2 * y
        };
    }

    public override string ToString()
    {
        return $"x:{X}, y:{Y}, w:{W}, h:{H}";
    }
}

public struct RectClip : IRenderFragment
{
    // private static readonly SKRoundRect RoundRect = new();

    public required Bounds Bounds { get; set; }
    public required float Radius;
    // public required SKClipOperation ClipOperation;

    public void Render(GlCanvas canvas)
    {
        if (Radius == 0)
        {
            canvas.ClipRect(Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
        }
        else
        {
            canvas.ClipRoundedRect(Bounds.X, Bounds.Y, Bounds.W, Bounds.H, Radius);
        }
    }

    public bool UiEquals(IRenderFragment renderFragment)
    {
        if (renderFragment is not RectClip rectClip)
            return false;

        return rectClip.Bounds.BoundsEquals(Bounds)
               && rectClip.Radius == Radius;
    }
}

public struct Rect : IRenderFragment, IClickableFragment
{
    public required UiElement UiElement { get; set; }
    public required Bounds Bounds { get; set; }
    public required float Radius;

    public void Render(GlCanvas canvas)
    {
        if (Radius == 0)
        {
            canvas.DrawRect(Bounds.X, Bounds.Y, Bounds.W, Bounds.H);
        }
        else
        {
            canvas.DrawRoundedRect(Bounds.X, Bounds.Y, Bounds.W, Bounds.H, Radius);
            // canvas.DrawRoundRect(Bounds.ToRect(), Radius, Radius, RenderPaint.GetPaint());
        }
    }

    public bool UiEquals(IRenderFragment renderFragment)
    {
        if (renderFragment is not Rect rect)
            return false;

        return rect.Bounds.BoundsEquals(Bounds)
               && rect.Radius == Radius;
    }
}

// public struct Bitmap : IRenderFragment
// {
//     public required Bounds Bounds;
//     public required SKBitmap SkBitmap;
//     private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();
//
//     public void Render(SKCanvas canvas)
//     {
//         canvas.DrawBitmap(SkBitmap, Bounds.ToRect(), Paint);
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Bitmap bitmap)
//             return false;
//
//         return bitmap.Bounds.BoundsEquals(Bounds);
//     }
// }

// public struct Picture : IRenderFragment //ToDo, should also be clickable
// {
//     public required SKPicture SkPicture;
//     public required SKMatrix SkMatrix;
//     public required string Src;
//
//     public void Render(SKCanvas canvas)
//     {
//         canvas.DrawPicture(SkPicture, ref SkMatrix);
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Picture pic)
//             return false;
//
//         return pic.Src == Src && SkMatrix == pic.SkMatrix;
//     }
// }

public struct Text : IRenderFragment //ToDo should also be clickable
{
    public required string Content;
    public required float X;
    public required float Y;

    public void Render(GlCanvas canvas)
    {
        canvas.DrawText(Content, X, Y, false);
    }

    public bool UiEquals(IRenderFragment renderFragment)
    {
        if (renderFragment is not Text text)
            return false;

        return text.Content == Content && text.X == X && text.Y == Y;
    }
}

public struct Matrix : IRenderFragment, IMatrixable
{
    public required Matrix4X4<float> SkMatrix;

    public void Render(GlCanvas canvas)
    {
        canvas.SetMatrix(SkMatrix);
        // canvas.SetMatrix(canvas.TotalMatrix.PostConcat(SkMatrix));
    }

    public bool UiEquals(IRenderFragment renderFragment)
    {
        if (renderFragment is not Matrix matrix)
            return false;

        return matrix.SkMatrix == SkMatrix;
    }

    public Vector2D<float> ProjectPoint(Vector2 point)
    {
        Matrix4X4.Invert(SkMatrix, out var m);
        var projectedPoint = Vector4D.Multiply(new Vector4D<float>(point.X, point.Y, 0, 0), m);
        return new Vector2D<float>(projectedPoint.X, projectedPoint.Y);
    }
}

// public struct Circle : IRenderFragment //ToDo, should also be clickable
// {
//     public required SKPaint SkPaint;
//     public required SKPoint Pos;
//     public required float Radius;
//
//     public void Render(SKCanvas canvas)
//     {
//         canvas.DrawCircle(Pos, Radius, SkPaint);
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Circle circle)
//             return false;
//
//         return circle.Pos == Pos && circle.Radius == Radius;
//     }
// }

// public struct Path : IRenderFragment //ToDo maybe should also be clickable
// {
//     public required SKPoint Start;
//     public required SKPoint StartHandle;
//     public required SKPoint EndHandle;
//     public required SKPoint End;
//     public required SKPaint SkPaint;
//
//     private static SKPath _path = new();
//
//     public void Render(SKCanvas canvas)
//     {
//         _path.MoveTo(Start);
//         _path.CubicTo(StartHandle, EndHandle, End);
//
//         canvas.DrawPath(_path, SkPaint);
//
//         _path.Reset();
//     }
//
//     public bool UiEquals(IRenderFragment renderFragment)
//     {
//         if (renderFragment is not Path path)
//             return false;
//
//         return path.Start == Start && path.StartHandle == StartHandle && path.EndHandle == EndHandle &&
//                path.End == End;
//     }
// }

// public struct TextPaint : IRenderPaint
// {
//     public required float TextSize;
//     public required SKColor SkColor;
//
//     private static readonly SKPaint Paint = MakeTextPaint();
//
//     public SKPaint GetPaint()
//     {
//         Paint.TextSize = TextSize;
//         Paint.Color = SkColor;
//         return Paint;
//     }
//
//     public bool PaintEquals(IRenderPaint renderPaint)
//     {
//         if (renderPaint is not TextPaint textPaint)
//             return false;
//
//         return textPaint.TextSize == TextSize && textPaint.SkColor == SkColor;
//     }
//
//     private static SKPaint MakeTextPaint()
//     {
//         var paint = new SKPaint();
//         paint.IsAntialias = true;
//         paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Thin, SKFontStyleWidth.Normal,
//             SKFontStyleSlant.Upright);
//         return paint;
//     }
// }

// public struct ShadowPaint : IRenderPaint
// {
//     public required SKColor SkColor;
//     public required float ShadowSigma;
//
//     private static readonly Dictionary<float, SKMaskFilter> MaskFilterCache = new();
//     private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();
//
//     public SKPaint GetPaint()
//     {
//         if (!MaskFilterCache.TryGetValue(ShadowSigma, out var maskFilter))
//         {
//             //todo maybe ensure that not no many unused maskfilters get created??? because maskfilters are disposable :) AND immutable grrrr
//             maskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, ShadowSigma, false);
//             MaskFilterCache.Add(ShadowSigma, maskFilter);
//         }
//
//         Paint.Color = SkColor;
//         Paint.MaskFilter = maskFilter;
//         return Paint;
//     }
//
//     public bool PaintEquals(IRenderPaint renderPaint)
//     {
//         if (renderPaint is not ShadowPaint shadowPaint)
//             return false;
//
//         return shadowPaint.SkColor == SkColor && shadowPaint.ShadowSigma == ShadowSigma;
//     }
// }

// public struct PlaintPaint : IRenderPaint
// {
//     public required SKColor SkColor;
//     private static readonly SKPaint Paint = Helpers.GetNewAntialiasedPaint();
//
//     public SKPaint GetPaint()
//     {
//         Paint.Color = SkColor;
//         return Paint;
//     }
//
//     public bool PaintEquals(IRenderPaint renderPaint)
//     {
//         if (renderPaint is not PlaintPaint plaintPaint)
//             return false;
//
//         return plaintPaint.SkColor == SkColor;
//     }
// }

// public static class Helpers
// {
//     public static SKPaint GetNewAntialiasedPaint()
//     {
//         var paint = new SKPaint();
//         paint.IsAntialias = true;
//         return paint;
//     }
// }
