using System.Collections;
using System.ComponentModel;
using SkiaSharp;

namespace Demo.Test;

public enum SizeKind
{
    Percentage,
    Pixel
}

public abstract class RenderObject
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public SizeDefinition PWidth { get; set; } = new SizeDefinition(100, SizeKind.Percentage);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public SizeDefinition PHeight { get; set; } = new SizeDefinition(100, SizeKind.Percentage);
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedHeight { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedWidth { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedX { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedY { get; set; }

    public abstract void Render();
}

public record struct SizeDefinition(float Value, SizeKind Kind);

public record struct ColorDefinition(float Red, float Gree, float Blue, float Transparency);

public static class StaticStuff
{
    public static SKPaint Black = new()
    {
        Color = new SKColor(),
        IsAntialias = true
    };
}

public class TextDefinition
{
    public SizeDefinition Width { get; set; } = new(100, SizeKind.Percentage);
    public SizeDefinition Height { get; set; } = new(100, SizeKind.Percentage);
}

public interface IDefinition
{
}

public abstract class UiComponent
{
    public abstract Div Render();
}