namespace Flamui.UiElements;

public abstract class UiElementContainer : UiElement, IDisposable
{
    public List<UiElement> Children { get; set; } = new List<UiElement>();

    public void AddChild(UiElement uiElement)
    {
        uiElement.Reset();
        Children.Add(uiElement);
        uiElement.Parent = this;
    }

    public void OpenElement()
    {
        Children.Clear();
    }

    public void Dispose()
    {
        Tree.Ui.PopElement();
    }
}
