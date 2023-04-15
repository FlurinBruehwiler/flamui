using System.ComponentModel;

namespace Demo.Test;

public class Txt : RenderObject
{

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

    public override void Render()
    {
        
    }
}