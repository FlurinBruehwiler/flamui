namespace ImSharpUI.Component;

public abstract class UiComponent
{
    public UiComponent? Parent { get; set; }

    public abstract void Render();
}
