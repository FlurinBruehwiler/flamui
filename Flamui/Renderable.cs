using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Maths;
using Font = Flamui.Drawing.Font;

namespace Flamui;

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

public record struct Bounds
{
    public required float X;
    public required float Y;
    public required float W;
    public required float H;

    public float Right => X + W;

    [SetsRequiredMembers]
    public Bounds(Vector2 position, Vector2 size)
    {
        X = position.X;
        Y = position.Y;
        W = size.X;
        H = size.Y;
    }

    [SetsRequiredMembers]
    public Bounds(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        W = width;
        H = height;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(X, Y);
    }

    public Vector2 GetSize()
    {
        return new Vector2(W, H);
    }

    [Pure]
    public Bounds OffsetBy(Vector2 offsetPosition)
    {
        return new Bounds(GetPosition() + offsetPosition, GetSize());
    }

    [Pure]
    public bool ContainsPoint(Vector2 point)
    {
        var withinX = point.X >= X && point.X <= X + W;
        var withinY = point.Y >= Y && point.Y <= Y + H;

        return withinX && withinY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 TopLeft() => new(X, Y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 TopRight() => new(X + W, Y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 BottomLeft() => new(X, Y + H);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 BottomRight() => new(X + W, Y + H);

    //
    // public SKRect ToRect()
    // {
    //     return SKRect.Create(X, Y, W, H);
    // }

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
    Matrix,
    Path,
    Picture,
    TinyVG
}

public struct Command : IEquatable<Command>
{
    public CommandType Type;
    public GlPath Path;
    public ManagedRef<UiElement> UiElement;
    public ArenaString String;
    public Bounds Bounds;
    public float Radius;
    public ManagedRef<Font> Font;
    public float FontSize;
    public ColorDefinition Color;
    public Matrix4X4<float> Matrix;
    public Bitmap Bitmap;
    public int VGId;
    public Slice<byte> VGData;

    public bool Equals(Command other)
    {
        return Type == other.Type && UiElement.Equals(other.UiElement) && String.Equals(other.String) && Bounds.Equals(other.Bounds) && Radius.Equals(other.Radius) && Font.Equals(other.Font) && FontSize.Equals(other.FontSize) && Color.Equals(other.Color) && Matrix.Equals(other.Matrix);
    }

    public override bool Equals(object? obj)
    {
        return obj is Command other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add((int)Type);
        hashCode.Add(UiElement);
        hashCode.Add(String);
        hashCode.Add(Bounds);
        hashCode.Add(Radius);
        hashCode.Add(Font);
        hashCode.Add(FontSize);
        hashCode.Add(Color);
        hashCode.Add(Matrix);
        return hashCode.ToHashCode();
    }
}

public struct ManagedRef<T> : IEquatable<ManagedRef<T>> where T : class
{
    private IntPtr handle;

    public ManagedRef(IntPtr handle)
    {
        this.handle = handle;
    }

    [Pure]
    public T? Get()
    {
        if (handle != default)
        {
            return (T)GCHandle.FromIntPtr(handle).Target!;
        }

        return null;
    }

    public bool Equals(ManagedRef<T> other)
    {
        return Get() == other.Get();
    }

    public override bool Equals(object? obj)
    {
        return obj is ManagedRef<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Get().GetHashCode();
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