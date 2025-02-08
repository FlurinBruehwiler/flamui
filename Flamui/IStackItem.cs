using Flamui.UiElements;

namespace Flamui;

public interface IStackItem
{
    public DataStore DataStore { get; }
    public void AddChild(UiElement obj);
}
