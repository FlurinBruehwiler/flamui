# Components

With components, you can reuse UI elements. Components have the ability to store state and use Dependency Injection.

To define a component, create a class that inherits from `FlamuiComponent`.
```csharp
class IconButton(SomeService service) : FlamuiComponent
{
    private int counter;

    public override void Build(Ui ui)
    {
        ui.Text("Hello World");
    }
}
```

In the background, a source generator will create a method that can be used to build to component.
```csharp
ui.CreateIconButton().Build();
```

## Optional parameters
To pass an optional parameter to the component, define a property with the `[Parameter]` attribute

```csharp
[Parameter]
public int Number { get; set; }
```

This then can be set like this
```csharp
ui.CreateIconButton().Number(1).Build();
```

## Required parameters
To make a parameter required, add the required modifier to the attribute
```csharp
[Parameter]
public required int Number { get; set; }
```

You will then be forced to pass the parameter
```csharp
ui.CreateIconButton(1).Build();
```

## Ref parameters
If you want to modifications to the parameter to be seen by the parent, define the parameter as 'ref'

````csharp

````
