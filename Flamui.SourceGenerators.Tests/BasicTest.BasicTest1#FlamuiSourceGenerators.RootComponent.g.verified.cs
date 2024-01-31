//HintName: FlamuiSourceGenerators.RootComponent.g.cs
namespace Sample.ComponentGallery;

public partial class RootComponent
{
    public static RootComponent Create(string Input, System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)
    {
        RootComponent component = Flamui.Ui.GetComponent<RootComponent>(key, path, line);
        component.Input = Input;
        return component;
    }

    public RootComponent SetShouldShow(bool value){
        this.ShouldShow = value;
        return this;
    }
}
