namespace Flamui.UiElements;

public class EmptyStack : IStackItem
{
    public DataStore DataStore { get; } = new();

    public UiElement? UiElement { get; set; }

    public void AddChild(object obj)
    {
        if (obj is UiElement uiElement)
        {
            UiElement = uiElement;
        }
    }
}