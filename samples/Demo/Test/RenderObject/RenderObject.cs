using System.ComponentModel;

namespace Demo.Test.RenderObject;

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