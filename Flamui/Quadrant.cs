using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Flamui.Layouting;
using SkiaSharp;

namespace Flamui;

public record struct Quadrant(float Left, float Right, float Top, float Bottom);
public record struct AbsolutePosition(float? Left, float? Right, float? Top, float? Bottom);

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

public static class Extensions
{
    public static bool IsFlexible(this IUiElement uiElement, out FlexibleChildConfig config)
    {
        config = new FlexibleChildConfig();

        if (uiElement.FlexibleChildConfig is null)
        {
            return false;
        }

        config = uiElement.FlexibleChildConfig.Value;

        return true;
    }

    public static float GetMain(this Dir dir, float width, float height)
    {
        return dir switch
        {
            Dir.Horizontal => width,
            Dir.Vertical => height,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public static float Cross(this Dir dir, float width, float height)
    {
        return dir switch
        {
            Dir.Horizontal => height,
            Dir.Vertical => width,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
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

