using ImSharpUISample.UiElements;

namespace ImSharpUISample;

[AttributeUsage(AttributeTargets.Method)]
public class BuilderAttribute : Attribute
{

}

public class ModalComponent
{
    private Stack<UiContainer> _stackReserve;
    private UiContainer _root = new();

    [Builder]
    public void StartModal(ref bool show, string title)
    {
        _stackReserve = Ui.OpenElementStack;
        Ui.OpenElementStack = new Stack<UiContainer>();
        Ui.OpenElementStack.Push(_root);
    }

    [Builder]
    public void EndModal()
    {
        var modalContent = _root.Children;
        Ui.OpenElementStack = _stackReserve;
    }
}

public class DropDownComponent
{
    [Builder]
    public static void StartDropDown()
    {

    }

    [Builder]
    public static void DropDownEntry()
    {

    }

    [Builder]
    public static void EndDropDown()
    {

    }
}
