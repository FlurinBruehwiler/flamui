namespace Flamui.UiElements;

public class EmptyStack : IStackItem
{
    public DataStore DataStore { get; } = new();

    public FlexContainer? UiElement { get; set; }

    public void AddChild(object obj)
    {
        if (obj is FlexContainer uiElement)
        {
            UiElement = uiElement;
        }
    }
}