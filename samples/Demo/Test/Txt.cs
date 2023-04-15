using System.ComponentModel;

namespace Demo.Test;

public class Txt : IComponent
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public SizeDefinition PWidth { get; set; } = new(100, SizeKind.Percentage);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public SizeDefinition PHeight { get; set; } = new(100, SizeKind.Percentage);
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedHeight { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedWidth { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedX { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float PComputedY { get; set; }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string PTxt { get; set; }
    
    public Txt Content(string txt)
    {
        PTxt = txt;
        return this;
    }
    
    public Txt Width(float width, SizeKind sizeKind = SizeKind.Pixel)
    {
        PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }

    public Txt Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    {
        PHeight = new SizeDefinition(height, sizeKind);
        return this;
    }
}