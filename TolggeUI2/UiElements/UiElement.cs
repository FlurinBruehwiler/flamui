using SkiaSharp;

namespace TollgeUI2.UiElements;

public abstract class UiElement
{
    public UiElementId Id { get; init; }
    public SizeDefinition PWidth { get; set; } = new(100, SizeKind.Percentage);

    public SizeDefinition PHeight { get; set; } = new(100, SizeKind.Percentage);

    public float PComputedHeight { get; set; }

    public float PComputedWidth { get; set; }

    public float PComputedX { get; set; }

    public float PComputedY { get; set; }
    public abstract void Render(SKCanvas canvas);
    public abstract void Layout();
}

public record struct UiElementId(string Key, string Path, int Line);
