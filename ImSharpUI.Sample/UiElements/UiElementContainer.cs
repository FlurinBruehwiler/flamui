namespace ImSharpUISample.UiElements;

public abstract class UiElementContainer : UiElement
{
    public List<UiElement> Children { get; set; } = new();

    public Dictionary<UiElementId, UiElement> OldChildrenById { get; set; } = new();
    private Dictionary<UiElementId, object>? _oldDataById;
    private List<Data>? _data;
    public Dictionary<UiElementId, object> OldDataById => _oldDataById ??= new Dictionary<UiElementId, object>();
    public List<Data> Data => _data ??= new List<Data>();//maybe change do dictionary, but maybe this is slower, should benchmark it

    public T AddChild<T>(UiElementId uiElementId) where T : UiElement, new()
    {
        if (OldChildrenById.TryGetValue(uiElementId, out var child))
        {
            Children.Add(child);
            return (T)child;
        }

        var newChild = new T
        {
            Id = uiElementId
        };

        Console.WriteLine($"Created {newChild.Id}");

        Children.Add(newChild);
        return newChild;
    }

    public virtual void OpenElement()
    {
        OldChildrenById.Clear();
        foreach (var uiElementClass in Children)
        {
            OldChildrenById.Add(uiElementClass.Id, uiElementClass);
        }

        Children.Clear();


        OldDataById.Clear();
        foreach (var o in Data)
        {
            OldDataById.Add(o.Id, o.Obj);
        }

        Data.Clear();
    }

    public virtual void CloseElement()
    {

    }
}
