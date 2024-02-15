using System.Diagnostics;

namespace Flamui.UiElements;

public interface IData
{
    public UiElementId Id { get; set; }
}

[DebuggerDisplay("Line = {Id.Line} Key = {Id.Key}")]
public abstract class UiElement : IData
{
    public UiElementId Id { get; set; }
    public UiElementContainer Parent { get; set; }
    public SizeDefinition PWidth { get; set; } = new(100, SizeKind.Percentage);
    public SizeDefinition PHeight { get; set; } = new(100, SizeKind.Percentage);

    public Bounds ComputedBounds;
    public UiWindow Window { get; set; }

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
    public abstract bool LayoutHasChanged();
    public abstract bool HasChanges();

    public virtual void CleanElement()
    {

    }
}

public record struct UiElementId(string Key, string Path, int Line);

public record struct Size(float Width, float Heght);
