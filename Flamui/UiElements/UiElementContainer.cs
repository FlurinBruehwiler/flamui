using System.Numerics;

namespace Flamui.UiElements;

public abstract class UiElementContainer : UiElement, IDisposable, IStackItem
{
    public List<UiElement> Children { get; set; } = new();

    public DataStore DataStore { get; } = new();

    public void AddChild(object obj)
    {
        if (obj is UiElement uiElement)
        {
            Children.Add(uiElement);
            uiElement.Parent = this;
        }
    }

    public virtual Vector2 ProjectPoint(Vector2 point)
    {
        return point;
    }

    public void OpenElement()
    {
        DataStore.Reset();
        Children.Clear();
    }

    public bool ContainsPoint(Vector2 pos)
    {
        return ComputedBounds.X <= pos.X && ComputedBounds.X + ComputedBounds.W >= pos.X && ComputedBounds.Y <= pos.Y &&
               ComputedBounds.Y + ComputedBounds.H >= pos.Y;
    }

    public void Dispose()
    {
        Window.Ui.OpenElementStack.Pop();
    }
}
