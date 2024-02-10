//HintName: FlamuiSourceGenerators.Sample.ComponentGallery.RootComponent_T_.g.cs
namespace Sample.ComponentGallery;

public static partial class UiExtensions
{
    public static RootComponentBuilder<T> CreateRootComponent<T>(this Flamui.Ui ui, T IsEnabled, System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)
    {
        var component = ui.GetComponent<Sample.ComponentGallery.RootComponent<T>>(key, path, line);
        component.IsEnabled = IsEnabled;
        return new RootComponentBuilder<T>(ui, component);
    }
}
