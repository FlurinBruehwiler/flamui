using System.Diagnostics.Contracts;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Arenas;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Maths;

namespace Flamui;

public class RenderContext
{
    // public Dictionary<int, RenderSection> RenderSections = new();
    public Stack<int> ZIndexes = new();

    public RenderContext()
    {
        ZIndexes.Push(0);
        Reset();
    }

    private Arena _arena;

    public void Reset()
    {
        _arena = new Arena();

        // foreach (var (key, value) in RenderSections)
        // {
        //     value.Renderables.Clear();
        // }
    }

    public void AddRect(Bounds bounds, UiElement uiElement, ColorDefinition color, float radius = 0)
    {
        var cmd = new Command();
        cmd.UiElement.Set(_arena, uiElement);
        cmd.Bounds = bounds;
        cmd.Radius = radius;
        cmd.Type = CommandType.Rect;
        cmd.Color = color;

        Add(cmd);
    }

    public void AddClipRect(Bounds bounds, float radius = 0)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Radius = radius;
        cmd.Type = CommandType.ClipRect;

        Add(cmd);
    }

    public void AddText(Bounds bounds, string text, ColorDefinition color, Font font)
    {
        var cmd = new Command();
        cmd.Bounds = bounds;
        cmd.Type = CommandType.Text;
        cmd.String.Set(_arena, text);
        cmd.Color = color;
        cmd.Font.Set(_arena, font);

        Add(cmd);
    }

    public void AddMatrix(Matrix4X4<float> matrix)
    {
        var cmd = new Command();
        cmd.Matrix = matrix;
        cmd.Type = CommandType.Matrix;

        Add(cmd);
    }

    public Dictionary<int, ArenaList<Command>> RenderSections = []; //todo write custom collection, arena linked list....

    public void Add(Command command)
    {
        if (!RenderSections.TryGetValue(ZIndexes.Peek(), out var renderSection))
        {
            renderSection = new ArenaList<Command>(_arena, 20);
            RenderSections.Add(ZIndexes.Peek(), renderSection);
        }

        renderSection.Add(command);
    }

    public bool RequiresRerender(RenderContext lastRenderContext)
    {
        foreach (var (key, currentRenderSection) in RenderSections)
        {
            if (!lastRenderContext.RenderSections.TryGetValue(key, out var lastRenderSection))
            {
                return true;
            }

            if (lastRenderSection.Count != currentRenderSection.Count)
            {
                return true;
            }

            //memcmp...
            currentRenderSection.AsSpan().SequenceEqual(currentRenderSection.AsSpan());
        }

        return false;
    }

    private static int rerenderCount;

    public void Rerender(GlCanvas canvas)
    {
        var sections = RenderSections.OrderBy(x => x.Key).ToList();

        foreach (var (_, value) in sections)
        {
            foreach (var command in value)
            {
                switch (command.Type)
                {
                    case CommandType.Rect:
                        canvas.Paint.Color = Color.FromArgb(command.Color.Alpha, command.Color.Red, command.Color.Green, command.Color.Blue);
                        if(command.Radius == 0)
                            canvas.DrawRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H);
                        else
                            canvas.DrawRoundedRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H, command.Radius);
                        break;
                    case CommandType.ClipRect:
                        if(command.Radius == 0)
                            canvas.ClipRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H);
                        else
                            canvas.ClipRoundedRect(command.Bounds.X, command.Bounds.Y, command.Bounds.W, command.Bounds.H, command.Radius);
                        break;
                    case CommandType.Text:
                        canvas.Paint.Font = command.Font.Get<Font>(); //todo make font
                        canvas.DrawText(command.String.Get<string>(), command.Bounds.X, command.Bounds.Y);
                        break;
                    case CommandType.Matrix:
                        canvas.SetMatrix(command.Matrix);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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

    [Pure]
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

public enum CommandType : byte
{
    Rect,
    ClipRect,
    Text,
    Matrix
}

public struct Command
{
    public CommandType Type;
    public ManagedRef UiElement;
    public ManagedRef String;
    public Bounds Bounds;
    public float Radius;
    public ManagedRef Font;
    public ColorDefinition Color;
    public Matrix4X4<float> Matrix;
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