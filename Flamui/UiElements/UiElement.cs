using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Flamui.UiElements;

[DebuggerDisplay("Line = {Id.Line} Key = {Id.Key}")]
public abstract class UiElement
{
    public required UiID Id { get; init; }

    public UiElementContainer Parent { get; set; }

    public required UiWindow Window { get; init; }

    public SizeDefinition PWidth { get; set; }
    public SizeDefinition PHeight { get; set; }

    public SizeDefinition GetMainAxisSize()
    {
        if (Parent is UiContainer uiElement)
        {
            if (uiElement.PDir == Dir.Horizontal)
            {
                return PWidth;
            }

            return PHeight;
        }

        throw new Exception();
    }

    public Bounds ComputedBounds;

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
    public abstract void Render(RenderContext renderContext);
    public abstract void Layout();

    public abstract void CleanElement();
}

public record struct UiID(string Key, string Path, int Line, int TypeHash)
{
    public override string ToString()
    {
        return $"Key: {Key}, Path: {Path}, Line: {Line}, Type: {TypeHash}";
    }
}

public record struct Size(float Width, float Height);
