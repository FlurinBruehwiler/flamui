using Flamui.UiElements;

namespace Flamui;

public interface IStackItem
{
    public UiID Id { get; }
    public DataStore DataStore { get;}
    public void AddChild(object obj);
}
