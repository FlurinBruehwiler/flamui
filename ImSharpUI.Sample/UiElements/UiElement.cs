using SkiaSharp;

namespace ImSharpUISample.UiElements;

public abstract class UiElement : IData
{
    public UiElementId Id { get; set; }
    public SizeDefinition PWidth { get; set; } = new(100, SizeKind.Percentage);

    public SizeDefinition PHeight { get; set; } = new(100, SizeKind.Percentage);

    public float PComputedHeight { get; set; }

    public float PComputedWidth { get; set; }

    public float ComputedX { get; set; }

    public float ComputedY { get; set; }
    public abstract void Render(SKCanvas canvas);
    public abstract void Layout(UiWindow uiWindow);
    public abstract bool LayoutHasChanged();
    public abstract bool HasChanges();
}

public record struct UiElementId(string Key, string Path, int Line);
