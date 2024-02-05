//HintName: FlamuiSourceGenerators.ComponentGallery.g.cs
namespace ComponentGallery;

public partial struct RootComponentBuilder
{
    private Sample.ComponentGallery.RootComponent _component;

    public RootComponentBuilder(System.String Input, System.Boolean IsEnabled, System.Single Average, System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)
    {
        _component = Flamui.Ui.GetComponent<RootComponent>(key, path, line);
        _component.Input = Input;
        _component.IsEnabled = IsEnabled;
        _component.Average = Average;
    }

    public unsafe RootComponentBuilder Median(System.Single value){
        _component.Median = Median;
        return this;
    }

    public unsafe RootComponentBuilder ShouldShow(System.Boolean value){
        _component.ShouldShow = ShouldShow;
        return this;
    }

    public void Build()
    {
        _component.Build();
    }
}
