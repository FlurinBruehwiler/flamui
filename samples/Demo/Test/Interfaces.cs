using System.Collections;
using SkiaSharp;

namespace Demo.Test;

public enum SizeKind
{
    Percentage,
    Pixel
}

public interface IComponent
{
}

public class Txt : IComponent
{
}

public record struct SizeDefinition(int Value, SizeKind Kind);

public record struct ColorDefinition(int Red, int Gree, int Blue, int Transparency);

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

public class DivDefinition
{
    public List<DivDefinition> Children { get; set; } = new();
    public SizeDefinition Width { get; set; } = new(100, SizeKind.Percentage);
    public SizeDefinition Height { get; set; } = new(100, SizeKind.Percentage);
    public ColorDefinition Color { get; set; } = new(0, 0, 0, 255);
    public int Padding { get; set; }
    public int Gap { get; set; }
    public int Radius { get; set; }
    public int BorderWidth { get; set; }
    public Dir Dir { get; set; } = Dir.Column;
    public MAlign MAlign { get; set; } = MAlign.FlexStart;
    public XAlign XAlign { get; set; } = XAlign.FlexStart;
    public int ComputedHeight { get; set; }
    public int ComputedWidth { get; set; }
    public int ComputedX { get; set; }
    public int ComputedY { get; set; }
}

public abstract class UiComponent
{
    public abstract Div Render();
}