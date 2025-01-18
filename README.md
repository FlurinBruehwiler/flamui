# Flamui
A C# immediate mode desktop GUI framework.

**This Project is in very early development, not usable in any way!**

Immediate mode means that you don't have to worry about state, the entire UI gets rebuilt every frame.

The framework uses a custom OpenGl Renderer and GLFW to manage Windows, Input, Graphics Context, etc. via [Silk.Net](https://github.com/dotnet/Silk.NET) Bindings.

## Sample App Demo
https://github.com/FlurinBruehwiler/flamui/assets/47397416/cb77530a-873b-417c-aee7-f64d6607793c

## Nuget Packages (outdated...)
- [Flamui](https://www.nuget.org/packages/Flamui) 
- [Flamui.Components](https://www.nuget.org/packages/Flamui.Components)

## Basic Hello World

````
dotnet new console
dotnet add package Flamui --prerelease
````

*Program.cs*
```csharp
using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Hello World Window");

app.Run();

public class RootComponent : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        ui.Text("Hello World");
    }   
}
```

For more examples, look [here](./docs/Examples.md).
For general documentation, look in the /docs folder.
