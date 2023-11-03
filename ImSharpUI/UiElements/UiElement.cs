using System.Diagnostics;
using SkiaSharp;

namespace ImSharpUISample.UiElements;

[DebuggerDisplay("Line = {Id.Line} Key = {Id.Key}")]
public abstract class UiElement : IData
{
    public UiElementId Id { get; set; }
    public SizeDefinition PWidth { get; set; } = new(100, SizeKind.Percentage);

    public SizeDefinition PHeight { get; set; } = new(100, SizeKind.Percentage);

    public float ComputedHeight { get; set; }

    public float ComputedWidth { get; set; }

    public float ComputedX { get; set; }

    public float ComputedY { get; set; }
    public abstract void Render(RenderContext renderContext);
    public abstract void Layout(UiWindow uiWindow);
    public abstract bool LayoutHasChanged();
    public abstract bool HasChanges();

    public virtual void CleanElement()
    {

    }
}

public record struct UiElementId(string Key, string Path, int Line);
