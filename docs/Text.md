# Text

//ToDo CascasingValue to set size, font, and color

```csharp
ui.Text("Hello World");
```

## Size


## Font


## Color


## Layout
A text UI Element will allways try to occupy as much space as possible. This can't be changed and the only way to work around it is to create a wrapper div that defines the size.

```csharp
using(ui.Div().Height(30).WidthFraction(50))
{
    ui.Text("Hello World");
}
```
