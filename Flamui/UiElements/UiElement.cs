using System.Diagnostics;
using Flamui.Layouting;

namespace Flamui.UiElements;

[DebuggerDisplay("Line = {Id.Line} Key = {Id.Key}")]
public abstract class UiElement
{
    //----- Data ------
    public required UiID Id { get; init; }
    public UiElementContainer Parent { get; set; }
    public required UiWindow Window { get; init; }
    public bool IsActive;
    // public Bounds ComputedBounds;
    public ParentData ParentData { get; set; }
    public FlexibleChildConfig? FlexibleChildConfig { get; set; }
    public BoxSize BoxSize { get; set; }

    //----- Methods ------
    public abstract BoxSize Layout(BoxConstraint constraint);
    public abstract void Render(RenderContext renderContext);

    public virtual void PrepareLayout()
    {
        
    }

    public UiElement? GetPreviousSibling()
    {
        if (Parent == null!)
            return null;

        var indexOfChild = Parent.Children.IndexOf(this);
        if (indexOfChild != 0)
        {
            return Parent.Children[indexOfChild - 1];
        }

        return null;
    }

    public UiElement? GetNextSibling()
    {
        if (Parent == null!)
            return null;

        var indexOfChild = Parent.Children.IndexOf(this);
        if (indexOfChild != Parent.Children.Count - 1)
        {
            return Parent.Children[indexOfChild + 1];
        }

        return null;
    }
}

public record struct UiID(string Key, string Path, int Line, int TypeHash)
{
    public override string ToString()
    {
        return $"Key: {Key}, Path: {Path}, Line: {Line}, Type: {TypeHash}";
    }
}
