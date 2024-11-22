using System.Numerics;

namespace Flamui.UiElements;

public abstract class UiElementContainer : UiElement, IDisposable, IStackItem
{
    public List<UiElement> Children { get; set; } = new List<UiElement>();

    public DataStore DataStore { get; } = new();

    public void AddChild(UiElement uiElement)
    {
        uiElement.Reset();
        Children.Add(uiElement);
        uiElement.Parent = this;
    }

    public virtual void OpenElement()
    {
        DataStore.Reset();
        Children.Clear();
    }

    public void Dispose()
    {
        Window.Ui.OpenElementStack.Pop();
    }
}
