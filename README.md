# flamui
A C# immediate mode desktop GUI framework.

**This project is still in development!!!**
The progress until the first prerelease is tracked [here](https://github.com/FlurinBruehwiler/flamui/issues/13).

Immediate mode means that you don't have to worry about state, the entire UI gets rebuilt every frame.

The framework uses Skia for the rendering and SDL2 to manage Windows, Input, Graphics Context, etc

## Sample App Demo

https://github.com/FlurinBruehwiler/flamui/assets/47397416/cb77530a-873b-417c-aee7-f64d6607793c

## Basic Hello World

````
dotnet new console
dotnet add Flamui
````

*Program.cs*
```csharp
using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Hello World Window");

app.Run();

class RootComponent : FlamuiComponent
{
    public override void Build(Ui ui)
    {
        ui.Text("Hello World");
    }   
}
```
