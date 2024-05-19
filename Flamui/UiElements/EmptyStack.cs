namespace Flamui.UiElements;

public class EmptyStack : IStackItem
{
    public DataStore DataStore { get; } = new();

    public FlexContainer? UiElement { get; set; }

    public void AddChild(UiElement obj)
    {
        obj.Reset();

        if (obj is FlexContainer uiElement)
        {
            UiElement = uiElement;
        }
    }
}
