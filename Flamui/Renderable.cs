using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Maths;
using Font = Flamui.Drawing.Font;

namespace Flamui;

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

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 TopLeft() => new(X, Y);
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 TopRight() => new(X + W, Y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public Vector2 BottomLeft() => new(X, Y + H);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public Vector2 BottomRight() => new(X + W, Y + H);

    public override string ToString()
    {
        return $"x:{X}, y:{Y}, w:{W}, h:{H}";
    }
}

public enum CommandType : int
{
    Rect,
    ClipRect,
    Text,
    Matrix,
    Path,
    Picture,
    TinyVG,
    ClearClip
}

//make a more compact growable arena buffer,
//that can hold different sized structs, this then wouldn't support indexing, but we don't need that anyway

public struct RectCommand
{
    public required Bounds Bounds;
    public required float Radius;
    public required ColorDefinition Color;
    public required float BlurRadius;

}

public struct ClipRectCommand
{
    public required Bounds Bounds;
    public required float Radius;
    public required ClipMode ClipMode;

}

public struct TextCommand
{
    public required Bounds Bounds;
    public required ArenaString String;
    public required ColorDefinition Color;
    public required ManagedRef<Font> Font;
    public required float FontSize;
}

public struct MatrixCommand
{
    public required Matrix4X4<float> Matrix;

}

public struct TinyVGCommand
{
    public required Bounds Bounds;
    public required int VGId;
    public required Slice<byte> VGData;
}

public struct ClearClipCommand;

[StructLayout(LayoutKind.Explicit)]
public struct Command
{
    [FieldOffset(0)]
    public required CommandType Type;

    [FieldOffset(4)]
    public required int UiElementId;

    [FieldOffset(8)]
    public RectCommand RectCommand;
    [FieldOffset(8)]
    public ClipRectCommand ClipRectCommand;
    [FieldOffset(8)]
    public MatrixCommand MatrixCommand;
    [FieldOffset(8)]
    public TinyVGCommand TinyVGCommand;
    [FieldOffset(8)]
    public ClearClipCommand ClearClipCommand;
    [FieldOffset(8)]
    public TextCommand TextCommand;

    public UiElement? GetAssociatedUiElement(Ui ui)
    {
        if (UiElementId == 0)
            return null;

        if (ui.LastFrameDataStore.TryGetValue(UiElementId, out var value))
        {
            return (UiElement)value;
        }

        throw new Exception($"Element with ID {UiElementId} doesn't exist");
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        return Get()?.GetHashCode() ?? 0;
    }
}