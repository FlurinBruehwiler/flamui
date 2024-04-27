# Flamui
A C# immediate mode desktop GUI framework.

**The development of this project is currently paused.**

The progress until the first release is tracked [here](https://github.com/FlurinBruehwiler/flamui/issues/13).
Prereleases are available on Nuget, see the "Hello World" example below.

Immediate mode means that you don't have to worry about state, the entire UI gets rebuilt every frame.

The framework uses Skia for the rendering and SDL2 to manage Windows, Input, Graphics Context, etc

## Sample App Demo

https://github.com/FlurinBruehwiler/flamui/assets/47397416/cb77530a-873b-417c-aee7-f64d6607793c

## Nuget Packages
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
