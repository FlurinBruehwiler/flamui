using Flamui;

namespace Sample.ComponentGallery;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class Parameter2Attribute : System.Attribute
{
    public Parameter2Attribute(bool isRef = false)
    {

    }
}

public partial class RootComponent : FlamuiComponent
{
    [Parameter2(isRef:true)]
    public required string Input { get; set; }

    [Parameter]
    public bool ShouldShow { get; set; }

    public override void Build()
    {
    }
}

public unsafe partial struct RootComponentBuilder
{
    private RootComponent _component;
    private bool* _shouldShowPtr;

    public RootComponentBuilder(string input, System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)
    {
        _component = Flamui.Ui.GetComponent<RootComponent>(key, path, line);
        _component.Input = input;
    }

    public RootComponentBuilder ShouldShow(ref bool value){
        _component.ShouldShow = value;

        fixed (bool* ptr = &value)
        {
            _shouldShowPtr = ptr;
        }

        return this;
    }

    public void Build()
    {
        _component.Build();
        _component.ShouldShow = *_shouldShowPtr;
    }
}
