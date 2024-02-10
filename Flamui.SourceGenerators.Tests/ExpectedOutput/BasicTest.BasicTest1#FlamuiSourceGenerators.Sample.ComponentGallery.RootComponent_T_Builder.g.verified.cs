//HintName: FlamuiSourceGenerators.Sample.ComponentGallery.RootComponent_T_Builder.g.cs
namespace Sample.ComponentGallery;

public partial struct RootComponentBuilder<T>
{
    public Sample.ComponentGallery.RootComponent<T> Component { get; }
    private readonly Flamui.Ui _ui;

    public RootComponentBuilder(Flamui.Ui ui, Sample.ComponentGallery.RootComponent<T> component)
    {
        Component = component;
        _ui = ui;
    }

    public void Build(out T IsEnabled)
    {
        Component.Build(_ui);
        IsEnabled = Component.IsEnabled;
    }
}
