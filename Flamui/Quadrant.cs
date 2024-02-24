using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace Flamui;

public record struct Quadrant(int Left, int Right, int Top, int Bottom);
public record struct AbsolutePosition(int? Left, int? Right, int? Top, int? Bottom);

public enum SizeKind
{
    Percentage,
    Pixel,
    Shrink
}

public struct SizeDefinition
{
    public SizeDefinition(float value, SizeKind kind)
    {
        Value = value;
        Kind = kind;
    }

    public float GetDpiAwareValue()
    {
        if (Kind == SizeKind.Percentage)
            return Value;

        return Value;//Todo
    }

    public float Value { get; set; }
    public SizeKind Kind { get; set; }
}

public enum MAlign
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum Dir
{
    Horizontal,
    Vertical,
}

public enum XAlign
{
    FlexStart,
    FlexEnd,
    Center
}

public readonly record struct ColorDefinition(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
    public SKColor ToSkColor()
    {
        return new SKColor(Red, Green, Blue, Alpha);
    }

    public static ColorDefinition operator /(ColorDefinition original, byte opacity)
    {
        return original with
        {
            Alpha = (byte)(original.Alpha / opacity)
        };
    }
}

